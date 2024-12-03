using UnityEngine;
using System.IO;

public class ApplicationManager : MonoBehaviour
{
    // Function to quit the application
    public void QuitApplication()
    {
        Debug.Log("Application is quitting...");
        Application.Quit();

        // For editor testing (only works in the Unity Editor)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Function to clear cache (works only if Unity supports caching on the platform)
    public void ClearCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Cache cleared successfully!");
        }
        else
        {
            Debug.LogWarning("Failed to clear cache or cache was already cleared.");
        }
    }

    // Function to clear all data except config.json in StreamingAssets
    public void ClearAllDataExceptConfig()
    {
        // Clear Unity's cache
        ClearCache();

        // Clear PlayerPrefs (optional, be cautious)
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared!");

        // Path to persistent data directory
        string persistentDataPath = Application.persistentDataPath;

        // Path to StreamingAssets (where config.json is located)
        string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (Directory.Exists(persistentDataPath))
        {
            // Loop through all files in the directory
            string[] files = Directory.GetFiles(persistentDataPath);

            foreach (string file in files)
            {
                // Skip config.json located in StreamingAssets
                if (file != streamingAssetsPath)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"Deleted file: {file}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to delete {file}: {ex.Message}");
                    }
                }
            }

            // Optionally delete empty directories (if you want to clean up)
            string[] directories = Directory.GetDirectories(persistentDataPath);
            foreach (string dir in directories)
            {
                try
                {
                    Directory.Delete(dir, true); // Deletes the directory and its contents
                    Debug.Log($"Deleted directory: {dir}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to delete directory {dir}: {ex.Message}");
                }
            }
        }

        // After clearing, quit the application
        QuitApplication();
    }

    // Combine the functions: clear data and quit
    public void ClearCacheAndQuit()
    {
        ClearAllDataExceptConfig();
        QuitApplication();
    }
}
