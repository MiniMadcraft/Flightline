using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnRollLeftAction;
    public event EventHandler OnRollRightAction;
    public event EventHandler OnThrottleUpAction;
    public event EventHandler OnThrottleDownAction;
    public event EventHandler OnSpoilersAction;
    public event EventHandler OnPitchDownAction;
    public event EventHandler OnPitchUpAction;
    public event EventHandler OnFlapsDownAction;
    public event EventHandler OnFlapsUpAction;

    private bool firstApplicationFocusChange = true;

    public enum Binding // Create an enum containing all possible bindings
    {
        rollLeft,
        rollRight,
        throttleUp,
        throttleDown,
        spoilers,
        pitchDown,
        pitchUp,
        flapsEnable,
        flapsDisable
    }
    public PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Plane.Disable(); // Initially disable the action map

        playerInputActions.Plane.rollLeft.performed += RollLeft_performed; // Subscribe to the event
        playerInputActions.Plane.rollRight.performed += RollRight_performed;
        playerInputActions.Plane.throttleUp.performed += ThrottleUp_performed;
        playerInputActions.Plane.throttleDown.performed += ThrottleDown_performed;
        playerInputActions.Plane.spoilers.performed += Spoilers_performed;
        playerInputActions.Plane.pitchDown.performed += PitchDown_performed;
        playerInputActions.Plane.pitchUp.performed += PitchUp_performed;
        playerInputActions.Plane.flapsDown.performed += FlapsDown_performed;
        playerInputActions.Plane.flapsUp.performed += FlapsUp_performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Plane.rollLeft.performed -= RollLeft_performed; // Unsubscribe from the event
        playerInputActions.Plane.rollRight.performed -= RollRight_performed;
        playerInputActions.Plane.throttleUp.performed -= ThrottleUp_performed;
        playerInputActions.Plane.throttleDown.performed -= ThrottleDown_performed;
        playerInputActions.Plane.spoilers.performed -= Spoilers_performed;
        playerInputActions.Plane.pitchDown.performed -= PitchDown_performed;
        playerInputActions.Plane.pitchUp.performed -= PitchUp_performed;
        playerInputActions.Plane.flapsDown.performed -= FlapsDown_performed;
        playerInputActions.Plane.flapsUp.performed -= FlapsUp_performed;

        playerInputActions.Dispose(); // Dispose of the action map
    }

    private void RollLeft_performed(InputAction.CallbackContext obj) // When the input is done
    {
        OnRollLeftAction?.Invoke(this, EventArgs.Empty); // Invoke the event
    }
    private void RollRight_performed(InputAction.CallbackContext obj) // As above
    {
        OnRollRightAction?.Invoke(this, EventArgs.Empty);
    }
    private void ThrottleUp_performed(InputAction.CallbackContext obj) // As above
    {
        OnThrottleUpAction?.Invoke(this, EventArgs.Empty);
    }
    private void ThrottleDown_performed(InputAction.CallbackContext obj) // As above
    {
        OnThrottleDownAction?.Invoke(this, EventArgs.Empty);
    }
    private void Spoilers_performed(InputAction.CallbackContext obj) // As above
    {
        OnSpoilersAction?.Invoke(this, EventArgs.Empty);
    }
    private void PitchUp_performed(InputAction.CallbackContext obj) // As above
    {
        OnPitchUpAction?.Invoke(this, EventArgs.Empty);
    }
    private void PitchDown_performed(InputAction.CallbackContext obj) // As above
    {
        OnPitchDownAction?.Invoke(this, EventArgs.Empty);
    }
    private void FlapsUp_performed(InputAction.CallbackContext obj) // As above
    {
        OnFlapsUpAction?.Invoke(this, EventArgs.Empty);
    }
    private void FlapsDown_performed(InputAction.CallbackContext obj) // As above
    {
        OnFlapsDownAction?.Invoke(this, EventArgs.Empty);
    }

    private async void Start()
    {
        byte[] json = await CloudSaveService.Instance.Files.Player.LoadBytesAsync("PlayerInputActions"); // Fetch the action map file
        string overrides = System.Text.Encoding.UTF8.GetString(json); // Get the json text from the byte file
        playerInputActions.LoadBindingOverridesFromJson(overrides); // Load the binding overrides
        Debug.Log(overrides);
        playerInputActions.Plane.Enable(); // Enable the playerInputActions map
    }
    public string GetBindingText(Binding binding)
    {
        switch (binding) // Take in the given binding...
        {
            default:
            case Binding.rollLeft: // Check against each possible case...
                return playerInputActions.Plane.rollLeft.bindings[0].ToDisplayString(); // Return the binding as a readable string, i.e only "W" or "S"
            case Binding.rollRight:
                return playerInputActions.Plane.rollRight.bindings[0].ToDisplayString(); // As above for all
            case Binding.throttleUp:
                return playerInputActions.Plane.throttleUp.bindings[0].ToDisplayString();
            case Binding.throttleDown:
                return playerInputActions.Plane.throttleDown.bindings[0].ToDisplayString();
            case Binding.spoilers:
                return playerInputActions.Plane.spoilers.bindings[0].ToDisplayString();
            case Binding.pitchDown:
                return playerInputActions.Plane.pitchDown.bindings[0].ToDisplayString();
            case Binding.pitchUp:
                return playerInputActions.Plane.pitchUp.bindings[0].ToDisplayString();
            case Binding.flapsEnable:
                return playerInputActions.Plane.flapsDown.bindings[0].ToDisplayString();
            case Binding.flapsDisable:
                return playerInputActions.Plane.flapsUp.bindings[0].ToDisplayString();
        };
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        playerInputActions.Plane.Disable(); // Disable the action map to be able to rebind

        InputAction inputAction;

        switch (binding) // Take in the given binding
        {
            default:
            case Binding.rollLeft: // Check against each case...
                inputAction = playerInputActions.Plane.rollLeft; // Assign inputAction to the correct binding
                break;
            case Binding.rollRight:
                inputAction= playerInputActions.Plane.rollRight;
                break;
            case Binding.throttleUp:
                inputAction = playerInputActions.Plane.throttleUp;
                break;
            case Binding.throttleDown:
                inputAction = playerInputActions.Plane.throttleDown;
                break;
            case Binding.spoilers:
                inputAction = playerInputActions.Plane.spoilers;
                break;
            case Binding.pitchDown:
                inputAction = playerInputActions.Plane.pitchDown;
                break;
            case Binding.pitchUp:
                inputAction = playerInputActions.Plane.pitchUp;
                break;
            case Binding.flapsEnable:
                inputAction = playerInputActions.Plane.flapsDown;
                break;
            case Binding.flapsDisable:
                inputAction = playerInputActions.Plane.flapsUp;
                break;
        }
        inputAction.PerformInteractiveRebinding(0) // Perform an interactive binding on index 0 (All bindings only have 1 key so all are index 0)
            .OnComplete(async callback => // Interactive binding waits for the user to press a key. Once complete...
            {
                callback.Dispose(); // Dispose the callback as no longer needed
                playerInputActions.Plane.Enable(); // Enable the action map
                onActionRebound(); // Perform the onActionRebound() action, which calls the procedures that were passed into it
                string jsonData = playerInputActions.SaveBindingOverridesAsJson(); // Save the binding overrides as a json file
                string dataPath = Application.persistentDataPath + "/playerData.json"; // Temporarily store them in the persistent data path
                File.WriteAllText(dataPath, jsonData); // Write the json to the persistent data path
                byte[] file = File.ReadAllBytes(dataPath); // Create a byte file from this file
                await CloudSaveService.Instance.Files.Player.SaveAsync("PlayerInputActions", file); // Save the file to Unity's Cloud Save Service
            })
            .Start(); // Start the interactive rebinding..
    }

    private void Update()
    {
        if (playerInputActions.Plane.rollLeft.IsPressed()) { // If the key is pressed...
            OnRollLeftAction?.Invoke(this, EventArgs.Empty); // Invoke the event
        }
        if (playerInputActions.Plane.rollRight.IsPressed()) // As above
        {
            OnRollRightAction?.Invoke(this, EventArgs.Empty);
        }
        if (playerInputActions.Plane.throttleUp.IsPressed()) // As above
        {
            OnThrottleUpAction?.Invoke(this, EventArgs.Empty);
        }
        if (playerInputActions.Plane.throttleDown.IsPressed()) // As above
        {
            OnThrottleDownAction?.Invoke(this, EventArgs.Empty);
        }
        if (playerInputActions.Plane.pitchDown.IsPressed()) // As above
        {
            OnPitchDownAction?.Invoke(this, EventArgs.Empty);
        }
        if (playerInputActions.Plane.pitchUp.IsPressed()) // As above
        {
            OnPitchUpAction?.Invoke(this, EventArgs.Empty);
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3)) // If currently on the MainGameScene;
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !PauseUI.Instance.pauseUI.activeSelf) // If the escape key is pressed to bring up the pause menu, and the pause menu is not currently active
            {
                PauseUI.Instance.Show(); // Show the pause menu
            }
            if (!Application.isFocused && firstApplicationFocusChange)
            {
                Time.timeScale = 0f;
                firstApplicationFocusChange = false;
            }
            if (Application.isFocused && !firstApplicationFocusChange && !PauseUI.Instance.pauseUI.activeSelf)
            {
                Time.timeScale = 1f;
                firstApplicationFocusChange = true;
            }
        }
    }
}
