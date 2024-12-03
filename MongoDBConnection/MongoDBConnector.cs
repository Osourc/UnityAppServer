using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;
using System.IO;
using UnityEngine.EventSystems;

public class MongoDBConnector : MonoBehaviour
{
    private IMongoDatabase database;
    private MongoClient client;

    private string connectionString; // This will be set from config.json

    public GameObject playerPrefab;
    public Transform parentPanel;
    public TMP_InputField searchInputField; // Reference to the search input field
    public Button searchButton; // Reference to the search button

    private List<GameObject> playerDataPanels = new List<GameObject>();
    private List<Player> allPlayers = new List<Player>();  // Store all fetched players

    void Start()
    {
        connectionString = GetMongoConnectionString();  // Read the connection string from config.json
        ConnectToMongoDB();
        StartCoroutine(PeriodicFetchPlayerData());

        // Add listeners to search components
        searchButton.onClick.AddListener(OnSearchButtonClick);
        searchInputField.onEndEdit.AddListener(OnSearchInputEndEdit);
    }

    private string GetMongoConnectionString()
    {
        // Define the path to the config file
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        // Check if the config.json file exists
        if (File.Exists(configFilePath))
        {
            // Read the contents of the config.json file
            string json = File.ReadAllText(configFilePath);

            // Parse the JSON to get the MongoConnectionString value
            Config config = JsonUtility.FromJson<Config>(json);
            return config.MongoConnectionString;
        }
        else
        {
            Debug.LogError("config.json not found in StreamingAssets folder!");
            return string.Empty;
        }
    }

    private void ConnectToMongoDB()
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            Debug.LogError("MongoDB connection string is empty. Cannot connect.");
            return;
        }

        try
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase("Server");
            Debug.Log("Connected to MongoDB Atlas!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not connect to MongoDB Atlas: " + ex.Message);
        }
    }

    private IEnumerator PeriodicFetchPlayerData()
    {
        while (true)
        {
            FetchPlayerData();
            yield return new WaitForSeconds(5f); // Fetch data every 5 seconds
        }
    }

    private void FetchPlayerData()
    {
        var collection = database.GetCollection<Player>("Player");

        // Fetch and sort players by Username in ascending order
        var players = collection.Find(new BsonDocument())  // Fetch all players
                                 .Sort(Builders<Player>.Sort.Ascending("Username"))  // Sort by Username
                                 .ToList();

        allPlayers = players; // Store all players for later use

        DisplayPlayers(allPlayers); // Initially display all players
    }

    private void DisplayPlayers(List<Player> players)
    {
        // Clear existing player data panels
        foreach (var panel in playerDataPanels)
        {
            Destroy(panel);
        }
        playerDataPanels.Clear();

        // Display player data in panels
        foreach (var player in players)
        {
            GameObject playerDataPanel = Instantiate(playerPrefab, parentPanel);
            playerDataPanel.transform.localScale = Vector3.one;
            playerDataPanels.Add(playerDataPanel);

            TMP_Text userIdText = playerDataPanel.transform.Find("UserID")?.GetComponent<TMP_Text>();
            userIdText?.SetText(player._id);  // _id is a string, no conversion needed

            TMP_Text userNameText = playerDataPanel.transform.Find("UserName")?.GetComponent<TMP_Text>();
            userNameText?.SetText(player.Username);

            TMP_Text inGameNameText = playerDataPanel.transform.Find("InGameName")?.GetComponent<TMP_Text>();
            inGameNameText?.SetText(player.GameName);

            TMP_Text dnaCountText = playerDataPanel.transform.Find("DNACount")?.GetComponent<TMP_Text>();
            dnaCountText?.SetText(player.DnaCount.ToString());


            Button banButton = playerDataPanel.transform.Find("Ban")?.GetComponent<Button>();
            TMP_Text banButtonText = banButton?.GetComponentInChildren<TMP_Text>();
            if (banButton != null && banButtonText != null)
            {
                banButton.onClick.AddListener(() => ToggleBan(player._id, banButtonText));  // _id is a string

                if (player.IsBanned)
                {
                    banButtonText.text = "Unban";
                    banButtonText.color = Color.green;
                }
                else
                {
                    banButtonText.text = "Ban";
                    banButtonText.color = Color.red;
                }
            }
        }
    }

    public void OnSearchButtonClick()
    {
        string query = searchInputField.text;
        if (!string.IsNullOrEmpty(query))
        {
            // Perform search and filter the already fetched players
            List<Player> searchResults = SearchPlayers(query);

            if (searchResults.Count == 0)
            {
                ShakeSearchBar(); // Trigger the shake effect if no results
            }

            DisplayPlayers(searchResults); // Display only the search results
        }
        else
        {
            // If the search input is empty, display all players
            DisplayPlayers(allPlayers);
        }
    }

    private void OnSearchInputEndEdit(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            // If the search input is cleared, reload all players
            DisplayPlayers(allPlayers);
        }
        else
        {
            // Trigger search when user presses "Enter"
            List<Player> searchResults = SearchPlayers(text);

            if (searchResults.Count == 0)
            {
                ShakeSearchBar(); // Trigger the shake effect if no results
            }

            DisplayPlayers(searchResults); // Display only the search results
        }
    }

    public List<Player> SearchPlayers(string query)
    {
        // Search the already fetched players by Username or GameName (case-insensitive)
        return allPlayers.Where(player =>
            player.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            player.GameName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Toggle Ban logic
    private void ToggleBan(string playerId, TMP_Text banButtonText)
    {
        var collection = database.GetCollection<Player>("Player");
        var filter = Builders<Player>.Filter.Eq("_id", playerId);  // _id is a string
        var player = collection.Find(filter).FirstOrDefault();

        if (player != null)
        {
            bool isBanned = player.IsBanned;
            var update = Builders<Player>.Update.Set("IsBanned", !isBanned);
            collection.UpdateOne(filter, update);

            // Update UI
            banButtonText.text = isBanned ? "Ban" : "Unban";
            banButtonText.color = isBanned ? Color.red : Color.green;
        }
    }

    private void ShakeSearchBar()
    {
        // Start a coroutine to ensure the deselection happens after the frame update
        StartCoroutine(ShakeAndDeselect());
    }

    private IEnumerator ShakeAndDeselect()
    {
        // Wait until the next frame to ensure the current selection process is complete
        yield return null;

        // Remove focus from the input field
        EventSystem.current.SetSelectedGameObject(null);

        // Disable the input field
        searchInputField.interactable = false;

        // Change the input field's background color to red
        var image = searchInputField.GetComponent<Image>();
        if (image != null)
        {
            image.color = Color.red;
        }

        // Re-enable the input field after the shake duration
        yield return new WaitForSeconds(0.5f);  // Shake effect duration

        // Reset input field appearance
        if (image != null)
        {
            image.color = Color.white;
        }

        searchInputField.interactable = true;
    }

    [Serializable]
    public class Config
    {
        public string MongoConnectionString;
    }
}






public class Player
{
    [BsonId]
    public string _id { get; private set; }

    public string Username { get; set; }

    public string GameName { get; set; }

    public int DnaCount { get; set; }

    public bool IsBanned { get; set; }

    public List<string> InventoryItems { get; set; }

    [BsonExtraElements] // Skip over any extra fields not defined in this class
    public BsonDocument ExtraFields { get; set; }

}

