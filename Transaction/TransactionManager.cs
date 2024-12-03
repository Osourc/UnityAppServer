using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class TransactionFetcher : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInputField;
    public TMP_InputField dateInputField; // Added input field for date search
    public TMP_Text usernameDisplayText;
    public GameObject transactionPrefab;
    public Transform transactionContainer;
    public Button searchButton;

    [Header("MongoDB Settings")]
    private string connectionString = "mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0&ssl=true";
    private string databaseName = "Server";
    private string collectionName = "Player";

    private IMongoCollection<BsonDocument> playerCollection;
    private List<Transform> transactionEntries = new List<Transform>();

    private DateTime searchStartTime;
    private bool isDataActive = false;
    private const float DataExpiryTime = 1800f; // 30 minutes in seconds

    private void Start()
    {
        // Initialize MongoDB connection
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        playerCollection = database.GetCollection<BsonDocument>(collectionName);

        // Assign button functionality
        searchButton.onClick.AddListener(OnSearchButtonClicked);

        // Listen for input field changes
        usernameInputField.onValueChanged.AddListener(OnInputFieldChanged);
        dateInputField.onValueChanged.AddListener(OnDateInputChanged);

        // Hide the date input field initially
        dateInputField.gameObject.SetActive(false);

        // Listen for the Enter key to trigger search
        dateInputField.onEndEdit.AddListener(OnDateSearch);
    }

    private void Update()
    {
        // Clear data after 30 minutes
        if (isDataActive && (DateTime.Now - searchStartTime).TotalSeconds >= DataExpiryTime)
        {
            ClearPreviousTransactions();
            usernameDisplayText.text = string.Empty;
            isDataActive = false;
            Debug.Log("Transaction data cleared after 30 minutes.");
        }
    }

    private void OnSearchButtonClicked()
    {
        string username = usernameInputField.text;
        string dateFilter = dateInputField.text;

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(dateFilter))
        {
            Debug.LogWarning("Username or Date input is empty!");
            return;
        }

        FetchTransactions(username, dateFilter);
    }

    private void OnDateInputChanged(string newText)
    {
        // If the date input is cleared, just reset the date filter and keep transactions intact
        if (string.IsNullOrEmpty(newText))
        {
            Debug.Log("Date filter cleared. Keeping transactions.");
        }
        else
        {
            // Optionally, you could trigger a re-search if needed, or just keep the current transactions
            Debug.Log("Date filter updated.");
        }
    }

    private void OnInputFieldChanged(string newText)
    {
        // Clear transactions if the username input is cleared
        if (string.IsNullOrEmpty(newText))
        {
            ClearPreviousTransactions();
            usernameDisplayText.text = string.Empty;
            isDataActive = false;
            Debug.Log("Username input field cleared. Data removed. Please search again.");
        }
        else
        {
            // Optional: Trigger search again when the username changes (if you want this behavior)
            // OnSearchButtonClicked(); // Uncomment if you want to auto-search after username change
        }
    }

    private void OnDateSearch(string input)
    {
        // If user presses Enter in date input field, trigger search
        if (!string.IsNullOrEmpty(input))
        {
            OnSearchButtonClicked();
        }
    }

    private void FetchTransactions(string username, string dateFilter)
    {
        // Fetch player data by username
        var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
        var playerDocument = playerCollection.Find(filter).FirstOrDefault();

        if (playerDocument == null)
        {
            Debug.LogWarning($"No player found with username: {username}");
            return;
        }

        // Parse transactions and filter by date if provided
        if (playerDocument.Contains("Transactions"))
        {
            ClearPreviousTransactions();

            var transactions = playerDocument["Transactions"].AsBsonArray;
            bool transactionsFound = false;

            foreach (var transaction in transactions)
            {
                DateTime transactionDate = transaction["TransactionDate"].ToUniversalTime();
                int dnaChange = transaction["DnaChange"].AsInt32;

                // Filter transactions by the provided date if available
                if (IsTransactionMatchingDate(transactionDate, dateFilter))
                {
                    CreateTransactionUI(transactionDate, dnaChange);
                    transactionsFound = true;
                }
            }

            usernameDisplayText.text = $"Transactions for: {username}";

            // Show date input field if transactions are found
            if (transactionsFound)
            {
                dateInputField.gameObject.SetActive(true);
            }

            isDataActive = true;
            searchStartTime = DateTime.Now;
        }
        else
        {
            Debug.LogWarning("No transactions found for this player.");
        }
    }

    private bool IsTransactionMatchingDate(DateTime transactionDate, string dateFilter)
    {
        if (string.IsNullOrEmpty(dateFilter)) return true;

        // Try to parse partial date input
        try
        {
            if (dateFilter.Length == 4) // Year
            {
                int year = int.Parse(dateFilter);
                return transactionDate.Year == year;
            }
            else if (dateFilter.Length == 7) // Month and Year (e.g., "MM/yyyy")
            {
                DateTime monthDate = DateTime.ParseExact(dateFilter, "MM/yyyy", null);
                return transactionDate.Month == monthDate.Month && transactionDate.Year == monthDate.Year;
            }
            else if (dateFilter.Length == 10) // Full Date (e.g., "MM/dd/yyyy")
            {
                DateTime fullDate = DateTime.ParseExact(dateFilter, "MM/dd/yyyy", null);
                return transactionDate.Date == fullDate.Date;
            }
        }
        catch (FormatException)
        {
            Debug.LogWarning("Invalid date format.");
        }

        return false;
    }

    private void ClearPreviousTransactions()
    {
        foreach (Transform child in transactionContainer)
        {
            Destroy(child.gameObject);
        }
        transactionEntries.Clear();
        dateInputField.gameObject.SetActive(false);
    }

    private void CreateTransactionUI(DateTime date, int dnaChange)
    {
        GameObject newTransaction = Instantiate(transactionPrefab, transactionContainer);
        TMP_Text[] texts = newTransaction.GetComponentsInChildren<TMP_Text>();

        foreach (var text in texts)
        {
            if (text.name == "DateText")
                text.text = date.ToString("MM/dd/yyyy");
            else if (text.name == "DnaChangeText")
                text.text = dnaChange.ToString();
        }

        transactionEntries.Add(newTransaction.transform);
    }
}
