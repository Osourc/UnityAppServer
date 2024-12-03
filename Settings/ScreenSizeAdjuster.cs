using UnityEngine;

public class ScreenSizeAdjuster : MonoBehaviour
{
    public RectTransform[] backgroundPanels;

    void Start()
    {
        AdjustPanelsToScreen();
        // Set the game to full screen and adapt to the device's native resolution
        Screen.SetResolution(1280, 720, false);
    }

    void Update()
    {
        // Toggle fullscreen with F11 key
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        // Block Print Screen key (often used to take screenshots)
        if (Input.GetKeyDown(KeyCode.Print))
        {
            Debug.Log("Print Screen key blocked!");
            // Optionally, prevent the default screenshot behavior or show a message
        }

        // Block Windows + Shift + S key combination (for Windows 10/11)
        // You can check for the combination of Shift + S keys in Update
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Blocked Windows + Shift + S (Snipping Tool)!");
            // Optionally, prevent the snipping tool from being triggered or show a message
        }
    }
    void AdjustPanelsToScreen()
    {
        foreach (RectTransform panel in backgroundPanels)
        {
            // Adjust each panel to match the screen size
            panel.anchorMin = Vector2.zero; // Bottom-left corner
            panel.anchorMax = Vector2.one; // Top-right corner
            panel.offsetMin = Vector2.zero; // No offset from bottom-left
            panel.offsetMax = Vector2.zero; // No offset from top-right
        }
    }
}
