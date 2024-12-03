using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using BCrypt.Net;  // Add BCrypt.NET NuGet package

public class Admin : MonoBehaviour
{
    private string connectionString = "mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0&ssl=true&tls=true&authMechanism=GSSAPI";
    private IMongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<BsonDocument> adminCollection;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public GameObject ServerPanel;
    public GameObject LoginPanel;
    public Button submitButton;
    public float cooldownTime = 5f;

    private int loginAttempts = 3;

    private void Start()
    {
        client = new MongoClient(connectionString);
        database = client.GetDatabase("Server");
        adminCollection = database.GetCollection<BsonDocument>("Admin");

        ServerPanel.SetActive(false);
        errorText.gameObject.SetActive(false);
        submitButton.interactable = true;

        passwordInput.contentType = TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();

        usernameInput.onSelect.AddListener(HideErrorText);
        passwordInput.onSelect.AddListener(HideErrorText);
    }

    public void OnSubmit()
    {
        string enteredUsername = usernameInput.text;
        string enteredPassword = passwordInput.text;

        if (string.IsNullOrWhiteSpace(enteredUsername) || string.IsNullOrWhiteSpace(enteredPassword))
        {
            errorText.text = "Please fill out both fields.";
            errorText.color = Color.red;
            errorText.gameObject.SetActive(true);
            return;
        }

        ValidateUser(enteredUsername, enteredPassword);
    }

    private void ValidateUser(string enteredUsername, string enteredPassword)
    {
        var user = adminCollection.Find<BsonDocument>(filter => filter["AdminUser"] == enteredUsername).FirstOrDefault();

        if (user != null)
        {
            string storedPassword = user["AdminPass"].AsString;

            // Check if password is plaintext (does not start with bcrypt prefixes)
            if (!storedPassword.StartsWith("$2a$") && !storedPassword.StartsWith("$2b$") && !storedPassword.StartsWith("$2y$"))
            {
                if (storedPassword == enteredPassword)
                {
                    // Password matches, hash it and update the database
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(enteredPassword);

                    var update = Builders<BsonDocument>.Update.Set("AdminPass", hashedPassword);
                    adminCollection.UpdateOne(filter => filter["AdminUser"] == enteredUsername, update);

                    Debug.Log("Password migrated to bcrypt successfully.");
                    OnSuccessfulLogin(user);
                }
                else
                {
                    HandleFailedLogin();
                }
            }
            else
            {
                // Assume password is bcrypt, verify it
                if (BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword))
                {
                    Debug.Log("Password matches!");
                    OnSuccessfulLogin(user);
                }
                else
                {
                    HandleFailedLogin();
                }
            }
        }
        else
        {
            HandleFailedLogin();
        }
    }

    private void OnSuccessfulLogin(BsonDocument user)
    {
        string storedToken = user.Contains("token") ? user["token"].AsString : null;
        DateTime? tokenExpiry = user.Contains("tokenExpiry") && user["tokenExpiry"].IsBsonDateTime
            ? user["tokenExpiry"].ToUniversalTime()
            : (DateTime?)null;

        if (storedToken != null && tokenExpiry.HasValue && tokenExpiry.Value > DateTime.Now)
        {
            errorText.text = "Login successful!";
            errorText.color = Color.green;
            errorText.gameObject.SetActive(true);
            StartCoroutine(DelayedTransition());
        }
        else
        {
            string newToken = GenerateToken();
            DateTime expiry = DateTime.Now.AddHours(24);

            var update = Builders<BsonDocument>.Update.Set("token", newToken).Set("tokenExpiry", expiry);
            adminCollection.UpdateOne(filter => filter["AdminUser"] == user["AdminUser"].AsString, update);

            errorText.text = "Login successful! Token generated.";
            errorText.color = Color.green;
            errorText.gameObject.SetActive(true);
            StartCoroutine(DelayedTransition());
        }
    }

    private string GenerateToken()
    {
        var random = new System.Random();
        byte[] tokenBytes = new byte[32];
        random.NextBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    private void InsertAdminUser()
    {
        string username = "LLDadmin2024";
        string password = "SSOadminPass2024";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var adminUser = new BsonDocument
        {
            { "AdminUser", username },
            { "AdminPass", hashedPassword },
            { "token", "" },
            { "tokenExpiry", DateTime.UtcNow.AddHours(24) }
        };

        adminCollection.InsertOne(adminUser);
    }

    private void HandleFailedLogin()
    {
        loginAttempts--;
        errorText.color = Color.red;
        errorText.gameObject.SetActive(true);

        if (loginAttempts > 0)
        {
            errorText.text = $"Invalid login. You have {loginAttempts} tries left.";
        }
        else
        {
            errorText.text = "Invalid login. You have reached the maximum number of tries. Please wait...";
            submitButton.interactable = false;
            usernameInput.text = "";
            passwordInput.text = "";
            usernameInput.interactable = false;
            passwordInput.interactable = false;
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator DelayedTransition()
    {
        yield return new WaitForSeconds(0.8f);
        ServerPanel.SetActive(true);
        LoginPanel.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    private IEnumerator Cooldown()
    {
        float remainingCooldown = cooldownTime;
        while (remainingCooldown > 0)
        {
            submitButton.GetComponentInChildren<TMP_Text>().text = $"Cooldown ({remainingCooldown:F1}s)";
            yield return new WaitForSeconds(0.1f);
            remainingCooldown -= 0.1f;
        }

        loginAttempts = 3;
        errorText.gameObject.SetActive(false);
        submitButton.interactable = true;
        usernameInput.interactable = true;
        passwordInput.interactable = true;
        usernameInput.text = "";
        passwordInput.text = "";
        submitButton.GetComponentInChildren<TMP_Text>().text = "Submit";
    }

    private void HideErrorText(string input)
    {
        errorText.gameObject.SetActive(false);
    }
}
