using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class GuestLoginHandler : MonoBehaviour
{
    public static GuestLoginHandler Instance { get ; private set; }
    private PlayerInputActions playerInputActions;
    public bool isSignedIn = false;

    FlightData flightData;
    // Start is called before the first frame update
    async void Start()
    {
        playerInputActions = new PlayerInputActions();
        Instance = this;
        try
        {
            await UnityServices.InitializeAsync(); // Initialize Unity's Services to be used
        }
        catch (Exception e)
        {
            Debug.LogException(e); // If connection not made, log the error
        }
    }

    private void Update()
    {
        if (AuthenticationService.Instance.IsSignedIn && isSignedIn == true) // On every update, check if the user is signed in
        {
            AuthenticationUITransitions.Instance.PlayFadeOut(); // Play the fade out transition
        }
    }
    public async Task SignInAnonymously()
    {
        try
        {
            AuthenticationService.Instance.ClearSessionToken(); // Clear the saved session token (i.e if there was a previously logged in user on this system saved in cache, clear it)
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Sign in anonymously
            Debug.Log("Sign in successful"); // If successful, log
            PlayerPrefs.SetString("PlayerName", "Guest");
            PlayerPrefs.Save();
            await CreatePlayerData();
            isSignedIn = true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex); // Log the error
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
    }
}
