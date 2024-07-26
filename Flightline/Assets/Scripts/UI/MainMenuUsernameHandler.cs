using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class MainMenuUsernameHandler : MonoBehaviour
{
    public static MainMenuUsernameHandler Instance { get; private set; }
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI usernameSubText;
    public TextMeshProUGUI welcomeText;
    private bool isFirstUpdate = true;

    private void Start()
    {
        Instance = this;
    }
    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            UpdateUsernameHolder();
        }
    }

    public void UpdateUsernameHolder()
    {
        string username = AuthenticationService.Instance.PlayerInfo.Username; // Fetch the players username
        string displayNameText = PlayerPrefs.GetString("PlayerName");
        if (username != null) // If there is a username
        {
            usernameSubText.text = "@" + username;
            usernameText.text = displayNameText.Remove(displayNameText.Length - 5); // Set the username text to their display name
            welcomeText.text = "time to fly, " + usernameText.text; // Update the welcomeText
        }
        else // If there isn't a username, i.e a guest
        {
            usernameText.text = "guest"; // Update the username text to "Guest"
            usernameSubText.text = "@guest";
            welcomeText.text = "time to fly, " + usernameText.text; // Update the welcomeText
        }
    }

    public void UpdateWelcomeText()
    {
        welcomeText.text = "time to fly, " + usernameText.text;
    }
}
