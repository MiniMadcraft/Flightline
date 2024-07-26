using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITransitions : MonoBehaviour
{
    public static UITransitions Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    public bool ShowUI(CanvasGroup backgroundImage, Transform containerTransform, Vector3 previousScale, string givenClass) // Takes in the container, previous scale and the class
    {
        if (backgroundImage.alpha != 1f) // If the alpha value is not yet 1
        {
            backgroundImage.alpha += (Time.unscaledDeltaTime * 4); // Increment the alpha value
        }
        if (containerTransform.localScale != new Vector3(1f, 1f, 1f)) // If the scale is not yet 1
        {
            containerTransform.localScale = new Vector3(previousScale.x + 0.05f, previousScale.y + 0.05f, previousScale.z + 0.05f); // Increment the scale
            switch (givenClass)
            {
                case "SettingsUI": // If this is for the settings class...
                    SettingsUI.Instance.previousScale = containerTransform.localScale; // Set the previousScale of settingsUI to this new scale
                    break;
                case "AircraftSelectionUI":
                    AircraftSelectionUI.Instance.previousScale = containerTransform.localScale; // As above
                    break;
                case "LeaderboardUI":
                    LeaderboardUI.Instance.previousScale = containerTransform.localScale; // As above
                    break;
                case "ProfileUI":
                    ProfileUI.Instance.previousScale = containerTransform.localScale; // As above
                    break;
                default:
                    break;
            }
        }
        if (containerTransform.localScale == new Vector3(1f, 1f, 1f) && backgroundImage.alpha == 1f) // If scale is 1 and alpha is 1
        {
            return false; // Return false so the class knows the transition has finished
        }
        return true; // Else return true so the transition plays for another update() cycle
    }

    public bool HideUI(CanvasGroup backgroundImage, Transform containerTransform, Vector3 previousScale, string givenClass)
    {
        backgroundImage.alpha -= (Time.unscaledDeltaTime * 6); // Decrement the alpha value
        containerTransform.localScale = new Vector3(previousScale.x - 0.1f, previousScale.y - 0.1f, previousScale.z - 0.1f); // Decrement the scale
        if (containerTransform.localScale == new Vector3(0.2f, 0.2f, 0.2f)) // If the scale is the minimum scale
        {
            switch (givenClass)
            {
                case "SettingsUI": // If this is for the settingsUI...
                    SettingsUI.Instance.previousScale = new Vector3(0.3f, 0.3f, 0.3f); // Set the previous scale to this new scale
                    break;
                case "AircraftSelectionUI":
                    AircraftSelectionUI.Instance.previousScale = new Vector3(0.3f, 0.3f, 0.3f); // As above
                    break;
                case "LeaderboardUI":
                    LeaderboardUI.Instance.previousScale = new Vector3(0.3f, 0.3f, 0.3f); // As above
                    break;
                case "ProfileUI":
                    ProfileUI.Instance.previousScale = new Vector3(0.3f, 0.3f, 0.3f); // As above
                    break;
                default:
                    break;
            }
            backgroundImage.alpha = 0f; // Set the alpha value to 0
            return false; // Return false so the class knows the transition is finished
        }
        switch (givenClass) // If it isn't the minimum scale yet
        {
            case "SettingsUI": // If this is for the settingsUI...
                SettingsUI.Instance.previousScale = containerTransform.localScale; // Set the previous scale to this new scale
                break;
            case "AircraftSelectionUI":
                AircraftSelectionUI.Instance.previousScale = containerTransform.localScale; // As above
                break;
            case "LeaderboardUI":
                LeaderboardUI.Instance.previousScale = containerTransform.localScale; // As above
                break;
            case "ProfileUI":
                ProfileUI.Instance.previousScale = containerTransform.localScale; // As above
                break;
            default:
                break;
        }
        return true; // Return true so the class knows to complete this transition for another update() cycle
    }
}
