using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine.UI;
using BCrypt.Net;
using System.IO;
using System.Threading.Tasks; // Add this for Task

public class FetchAndOpenUrl : MonoBehaviour
{
    public Button fetchButton; // Assign this in the Inspector
    private string connectionString;
    private string databaseName = "Server";
    private string collectionName = "Explorer";

    public GameObject Transactionpanel;

    void Start()
    {
        // Retrieve the connection string from the config file
        connectionString = GetMongoConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            Debug.LogError("Connection string not found in config file.");
        }

        // Set up button listener
        fetchButton.onClick.AddListener(OnFetchButtonClick);
    }

    private string GetMongoConnectionString()
    {
        // The file path for the config.json
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the file content
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON into a Config object
            var config = JsonUtility.FromJson<Config>(json);
            return config.MongoConnectionString;
        }
        else
        {
            Debug.LogError("Config file not found in StreamingAssets.");
            return string.Empty; // Return empty or a default connection string
        }
    }

    // Config class to match the structure of the JSON file
    [System.Serializable]
    public class Config
    {
        public string MongoConnectionString;
    }

    private async void OnFetchButtonClick()
    {
        // Fetch the URL from the MongoDB Explorer collection
        string url = await FetchUrlFromMongoDB();

        if (!string.IsNullOrEmpty(url))
        {
            // Hash the URL using BCrypt
            string hashedUrl = HashUrl(url);

            // Open the URL (or the hashed URL)
            Debug.Log("Opening URL: " + url);  // Show the original URL
            Application.OpenURL(url); // Opens the original URL

            // You can use hashedUrl in your logic, e.g., storing or logging it
            Debug.Log("Hashed URL: " + hashedUrl);
        }
    }

    private async Task<string> FetchUrlFromMongoDB()
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        // Find the first document (you can modify this to your needs)
        var document = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();

        if (document != null && document.Contains("Url"))
        {
            return document["Url"].AsString;
        }
        else
        {
            Debug.LogError("URL not found in MongoDB.");
            return string.Empty;
        }
    }

    private string HashUrl(string url)
    {
        // Hash the URL using BCrypt
        return BCrypt.Net.BCrypt.HashPassword(url);
    }

    public void OnOpen()
    {
        Transactionpanel.SetActive(true);
    }
    public void OnClose()
    {
        Transactionpanel.SetActive(false);
    }
}
