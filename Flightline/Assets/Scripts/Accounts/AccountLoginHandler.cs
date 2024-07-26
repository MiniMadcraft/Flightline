using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Services.Leaderboards;
using Newtonsoft.Json;

public class AccountLoginHandler : MonoBehaviour
{
    public static AccountLoginHandler Instance { get ; private set; }
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Button signInButton;
    public Button signUpButton;
    public Button visibilityButton;
    public Image openEyeImage;
    public Image closedEyeImage;

    private PlayerInputActions playerInputActions;
    public bool isLoggedIn = false;
    public bool isPasswordVisible = false;
    private bool acceptedSignIn = false;

    public FlightData flightData;

    string usernameText = "";
    string passwordText = "";

    public enum KeysToSave // Enum to hold all keybinds to be saved upon successful sign up
    {
        throttleUp,
        throttleDown,
        pitchUp,
        pitchDown,
        rollLeft,
        rollRight,
        flapsUp,
        flapsDown,
        spoilers
    }

    private void Awake()
    {
        Instance = this;
    }
    async void Start()
    {
        playerInputActions = new PlayerInputActions();
        try
        {
            await UnityServices.InitializeAsync(); // Initialize Unity's Services to be used
        }
        catch (Exception e)
        {
            Debug.LogException(e); // If connection not made, log the error
        }
        signInButton.onClick.AddListener(() => // On clicking the sign in button
        {
            SignIn(); // Call the sign in procedure
        });
        signUpButton.onClick.AddListener(() => // On clicking the sign up button
        {
            Create(); // Call the create procedure
        });
        visibilityButton.onClick.AddListener(() =>
        {
            isPasswordVisible = !isPasswordVisible;
            if (isPasswordVisible)
            {
                passwordField.contentType = TMP_InputField.ContentType.Standard;
                passwordField.textComponent.SetAllDirty();
            }
            else
            {
                passwordField.contentType = TMP_InputField.ContentType.Password;
                passwordField.textComponent.SetAllDirty();
            }
            openEyeImage.gameObject.SetActive(!openEyeImage.IsActive());
            closedEyeImage.gameObject.SetActive(!closedEyeImage.IsActive());
        });
    }

    void Update()
    {
        if (AuthenticationService.Instance.IsSignedIn && isLoggedIn == false && acceptedSignIn) // On every update, check if the user is signed in
        {
            isLoggedIn = true; // Set isLoggedIn to true
            AuthenticationUITransitions.Instance.PlayFadeOut(); // Play the fade out transition
        }
    }
    public async void Create()
    {
        usernameText = usernameField.text;
        passwordText = passwordField.text;
        await SignUpWithUsernamePassword(usernameText, passwordText); // Attempt to create a player account
        await CreatePlayerData(); // Then, create the user's data
    }

    public async void SignIn()
    {
        usernameText = usernameField.text;
        passwordText = passwordField.text;
        await SignInWithUsernamePassword(usernameText, passwordText); // Attempt to log in to the given account
    }
    async Task SignUpWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password); // Try signing up
            Debug.Log("SignUp is successful."); // If successful, log
            await AuthenticationService.Instance.UpdatePlayerNameAsync(AuthenticationService.Instance.PlayerInfo.Username);
            PlayerPrefs.SetString("PlayerName", username);
            PlayerPrefs.Save();
            acceptedSignIn = true;
        }
        catch (AuthenticationException ex) // If an error
        {
            // Compare error code to AuthenticationErrorCodes
            Debug.LogException(ex);
            Debug.Log(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            Debug.LogException(ex);
            AuthenticationUITransitions.Instance.errorPageMainText.text = "invalid username or password\n\nif creating an account, ensure your details match the required formats.\n\nif you have forgotten your password, or issues persist, please contact joshua via discord, @joshuajb";
            AuthenticationUITransitions.Instance.ErrorPage(); // Display the error page
        }
    }

    async Task SignInWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password); // Try signing in
            string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            if (playerName == null)
            {
                throw new RequestFailedException(10000, "Too many requests");
            }
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            Debug.Log("SignIn is successful.");
            acceptedSignIn = true;
        }
        catch (AuthenticationException ex)
        {
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Notify the player with the proper error message
            Debug.LogException(ex);
            if (ex.ErrorCode == 10000 || ex.ErrorCode == 50)
            {
                AuthenticationUITransitions.Instance.errorPageMainText.text = "timed out\n\nyou have attempted to sign in too many times in a short period of time\n\nplease wait 1 minute before attempting to sign in again";
                AuthenticationService.Instance.SignOut();
                AuthenticationService.Instance.ClearSessionToken();
            }
            else
            {
                AuthenticationUITransitions.Instance.errorPageMainText.text = "invalid username or password\n\nif creating an account, ensure your details match the required formats.\n\nif you have forgotten your password, or issues persist, please contact joshua via discord, @joshuajb";
            }
            AuthenticationUITransitions.Instance.ErrorPage(); // Display the error page
        }
    }

    public async Task CreatePlayerData()
    {
        playerInputActions.Plane.flapsUp.ApplyBindingOverride("<Keyboard>/o");
        playerInputActions.Plane.flapsDown.ApplyBindingOverride("<Keyboard>/l");
        playerInputActions.Plane.throttleUp.ApplyBindingOverride("<Keyboard>/r");
        playerInputActions.Plane.throttleDown.ApplyBindingOverride("<Keyboard>/f");
        playerInputActions.Plane.rollLeft.ApplyBindingOverride("<Keyboard>/a");
        playerInputActions.Plane.rollRight.ApplyBindingOverride("<Keyboard>/d");
        playerInputActions.Plane.pitchUp.ApplyBindingOverride("<Keyboard>/s");
        playerInputActions.Plane.pitchDown.ApplyBindingOverride("<Keyboard>/w");
        playerInputActions.Plane.spoilers.ApplyBindingOverride("<Keyboard>/v");
        string defaultBindings = playerInputActions.SaveBindingOverridesAsJson(); // Create all the default bindings, and save as a binding override JSON
        string dataPath = Application.persistentDataPath + "/playerData.json"; // Temporarily store at the persistent data path
        File.WriteAllText(dataPath, defaultBindings);
        byte[] file = File.ReadAllBytes(dataPath); // Read the data as a byte file
        await CloudSaveService.Instance.Files.Player.SaveAsync("PlayerInputActions", file); // Save the byte file to the user's Cloud Save
        var difficulty = new Dictionary<string, object> { { "Difficulty", "Simple" } }; // Create Dictionaries for each of the data items
        var terrainHeight = new Dictionary<string, object> { { "TerrainHeight", 5 } };
        var masterAudio = new Dictionary<string, object> { { "MasterAudio", 10 } };
        var musicAudio = new Dictionary<string, object> { { "MusicAudio", 5 } };
        var soundEffectAudio = new Dictionary<string, object> { { "SoundEffectAudio", 10 } };
        var accountCreationDate = new Dictionary<string, object> { { "AccountCreationDate", System.DateTime.Now.ToLongDateString() } };
        var aircraftSelection = new Dictionary<string, object> { { "AircraftSelection", "Eagle" } };
        string data = JsonConvert.SerializeObject(flightData);
        File.WriteAllText(dataPath, data);
        file = File.ReadAllBytes(dataPath);
        await CloudSaveService.Instance.Files.Player.SaveAsync("FlightData", file);
        await CloudSaveService.Instance.Data.Player.SaveAsync(difficulty); // Save each data item to it's own key
        await CloudSaveService.Instance.Data.Player.SaveAsync(terrainHeight);
        await CloudSaveService.Instance.Data.Player.SaveAsync(masterAudio);
        await CloudSaveService.Instance.Data.Player.SaveAsync(musicAudio);
        await CloudSaveService.Instance.Data.Player.SaveAsync(soundEffectAudio);
        await CloudSaveService.Instance.Data.Player.SaveAsync(accountCreationDate);
        await CloudSaveService.Instance.Data.Player.SaveAsync(aircraftSelection);
        await LeaderboardsService.Instance.AddPlayerScoreAsync("ScoreLeaderboard", 0);
    }
}
