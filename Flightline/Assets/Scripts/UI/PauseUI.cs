using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance {  get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] public GameObject pauseUI;
    [SerializeField] private CanvasGroup pauseUICanvasGroup;
    [SerializeField] GameObject freeLookCamera;

    private bool showUI = false;
    private bool hideUI = false;

    void Start()
    {
        Instance = this;
        resumeButton.onClick.AddListener(() => // On clicking the resume button
        {
            Hide(); // Hide the pause menu
        });
        settingsButton.onClick.AddListener(() => // On clicking the settings button
        {
            SettingsUI.Instance.Show(); // Show the settings UI
        });
        mainMenuButton.onClick.AddListener(() => // On clicking the main menu button
        {
            BackgroundUIMainGameTransition.Instance.TransitionOut();
        });
    }

    private void Update()
    {
        if (showUI) // If showing the pause menu
        {
            TransitionIn(); // Transition in
        }
        if (hideUI) // If hiding the pause menu
        {
            TransitionOut(); // Transition out
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !SettingsUI.Instance.hideUI && !SettingsUI.Instance.showUI && !hideUI && pauseUI.activeSelf && !showUI)
        {
            if (SettingsUI.Instance.backgroundImage.isActiveAndEnabled)
            {
                SettingsUI.Instance.Hide();
            }
            else
            {
                Hide();
            }
        }
    }
    public void Show()
    {
        showUI = true;
        pauseUI.SetActive(true); // Enable the game object
        freeLookCamera.SetActive(false);
    }

    private void Hide()
    {
        hideUI = true;
        Time.timeScale = 1f; // Reset time scale to 1
        freeLookCamera.SetActive(true);
    }

    private void TransitionIn()
    {
        if (pauseUICanvasGroup.alpha != 1f) // If alpha not yet 1
        {
            pauseUICanvasGroup.alpha += Time.unscaledDeltaTime * 3f; // Increment with respect to UNSCALED DELTA TIME
        }
        else
        {
            showUI = false; // Transition finished so set bool to false
            Time.timeScale = 0f; // Set time scale to 0
        }
    }

    private void TransitionOut()
    {
        if (pauseUICanvasGroup.alpha != 0f) // If alpha not yet 0
        {
            pauseUICanvasGroup.alpha -= Time.unscaledDeltaTime * 3f; // Decrement with respect to UNSCALED DELTA TIME
        }
        else
        {
            hideUI = false; // Transition finished so set bool to false
            pauseUI.SetActive(false); // Disable the game object
        }
    }
}
