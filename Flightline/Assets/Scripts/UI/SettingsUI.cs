using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave.Models;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [Header("General buttons + animations")]
    public Button backButton;
    public CanvasGroup backgroundImage;
    public Transform containerTransform;
    public bool showUI;
    public bool hideUI;
    private bool transition;
    public Vector3 previousScale = new Vector3(0.2f, 0.2f, 0.2f);
    private const string SETTINGS_UI = "SettingsUI";
    public GameObject generalPage;
    public GameObject audioPage;
    public GameObject aboutPage;
    public Button generalButton;
    public Button audioButton;
    public Button aboutButton;
    private GameObject currentPage;
    private CanvasGroup currentPageCanvasGroup;
    private GameObject pageToTransitionTo;
    private CanvasGroup pageToTransitionToCanvasGroup;
    public CanvasGroup pressToRebindKeyCanvasGroup;
    private bool showRebind = false;
    private bool hideRebind = false;
    private int currentTerrainHeight;
    private int currentMasterVolume;
    private int currentMusicVolume;
    private int currentSoundEffectVolume;

    [Header("Rebind Buttons + Button Text")]
    [SerializeField] private Button rollLeftButton;
    [SerializeField] private Button rollRightButton;
    [SerializeField] private Button throttleUpButton;
    [SerializeField] private Button throttleDownButton;
    [SerializeField] private Button spoilersButton;
    [SerializeField] private Button pitchDownButton;
    [SerializeField] private Button pitchUpButton;
    [SerializeField] private Button flapsEnableButton;
    [SerializeField] private Button flapsDisableButton;
    [SerializeField] private TextMeshProUGUI rollLeftText;
    [SerializeField] private TextMeshProUGUI rollRightText;
    [SerializeField] private TextMeshProUGUI throttleUpText;
    [SerializeField] private TextMeshProUGUI throttleDownText;
    [SerializeField] private TextMeshProUGUI spoilersText;
    [SerializeField] private TextMeshProUGUI pitchDownText;
    [SerializeField] private TextMeshProUGUI pitchUpText;
    [SerializeField] private TextMeshProUGUI flapsEnableText;
    [SerializeField] private TextMeshProUGUI flapsDisableText;
    [SerializeField] private Transform pressToRebindKeyTransform;

    [Header("Non-Rebind buttons + text + outlines")]
    [SerializeField] GameObject simplePhysicsOutline;
    [SerializeField] GameObject realisticPhysicsOutline;
    [SerializeField] TextMeshProUGUI terrainHeightText;
    [SerializeField] TextMeshProUGUI masterVolumeText;
    [SerializeField] TextMeshProUGUI musicVolumeText;
    [SerializeField] TextMeshProUGUI soundEffectVolumeText;
    [SerializeField] Button simplePhysicsButton;
    [SerializeField] Button realisticPhysicsButton;
    [SerializeField] Button increaseHeightButton;
    [SerializeField] Button decreaseHeightButton;
    [SerializeField] Button increaseMasterVolumeButton;
    [SerializeField] Button decreaseMasterVolumeButton;
    [SerializeField] Button increaseMusicVolumeButton;
    [SerializeField] Button decreaseMusicVolumeButton;
    [SerializeField] Button increaseSoundEffectVolumeButton;
    [SerializeField] Button decreaseSoundEffectVolumeButton;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gameObject.SetActive(false); // Initially set the game object inactive
        HidePressToRebindKey();
        backButton.onClick.AddListener(() => // When the back button is pressed
        {
            hideUI = true; // Set hideUI to true
        });
        generalButton.onClick.AddListener(() => // When the general button is pressed
        {
            if (currentPage != generalPage && !transition) // If the button pressed is not the currently displayed menu's button, and a transition is not currently playing
            {
                PageTransition(currentPage, generalPage); // Transition between the pages
            }
        });
        audioButton.onClick.AddListener(() => // When the audio button is pressed
        {
            if (currentPage != audioPage && !transition) // If the button pressed is not the currently displayed menu's button, and a transition is not currently playing
            {
                PageTransition(currentPage, audioPage); // Transition between the pages
            }
        });
        aboutButton.onClick.AddListener(() => // When the about button is pressed
        {
            if (currentPage != aboutPage && !transition) // If the button pressed is not the currently displayed menu's button, and a transition is not currently playing
            {
                PageTransition(currentPage, aboutPage); // Transition between the pages
            }
        });
        backgroundImage.alpha = 0f; // Initially set the alpha value to 0
        currentPage = generalPage; // Set the currentPage to the generalPage as the default

        // On keybind button click
        rollLeftButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.rollLeft); // Rebind the respective binding
        });
        rollRightButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.rollRight); // As above...
        });
        throttleUpButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.throttleUp); // As above...
        });
        throttleDownButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.throttleDown); // As above...
        });
        spoilersButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.spoilers); // As above...
        });
        pitchDownButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.pitchDown); // As above...
        });
        pitchUpButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.pitchUp); // As above...
        });
        flapsEnableButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.flapsEnable); // As above...
        });
        flapsDisableButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.flapsDisable); // As above...
        });

        // Other general buttons...

        simplePhysicsButton.onClick.AddListener(() =>
        {
            if (!simplePhysicsOutline.gameObject.activeSelf) // If the outline is not currently active on that button...
            {
                realisticPhysicsOutline.gameObject.SetActive(false); // Disable the outline on the realistic button..
                simplePhysicsOutline.gameObject.SetActive(true); // Enable it on this button
                var difficulty = new Dictionary<string, object> { { "Difficulty", "Simple" } };
                CloudSaveService.Instance.Data.Player.SaveAsync(difficulty); // Update the difficulty key to "Simple"
            }
        });
        realisticPhysicsButton.onClick.AddListener(() =>
        {
            if (!realisticPhysicsOutline.gameObject.activeSelf) // Same as above
            {
                realisticPhysicsOutline.gameObject.SetActive(true);
                simplePhysicsOutline.gameObject.SetActive(false);
                var difficulty = new Dictionary<string, object> { { "Difficulty", "Realistic" } };
                CloudSaveService.Instance.Data.Player.SaveAsync(difficulty);
            }
        });
        increaseHeightButton.onClick.AddListener(async () =>
        {
            if (currentTerrainHeight != 10) // If not at max height...
            {
                currentTerrainHeight++; // Increment
                terrainHeightText.text = currentTerrainHeight.ToString(); // Update display text
                var terrainHeightData = new Dictionary<string, object> { { "TerrainHeight", currentTerrainHeight} }; // Create a Dictionary to hold the updated "TerrainHeight" value
                await CloudSaveService.Instance.Data.Player.SaveAsync(terrainHeightData); // Update the respective value in the user's Cloud Save
            }
        });
        decreaseHeightButton.onClick.AddListener(async () =>
        {
            if (currentTerrainHeight != 1) // If not at min height...
            {
                currentTerrainHeight--; // Decrement
                terrainHeightText.text = currentTerrainHeight.ToString(); // Update display text
                var terrainHeightData = new Dictionary<string, object> { { "TerrainHeight", currentTerrainHeight } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(terrainHeightData); // Save as above
            }
        });
        increaseMasterVolumeButton.onClick.AddListener(async () =>
        {
            if (currentMasterVolume != 10) // If not at max volume..
            {
                currentMasterVolume++; // Increment
                masterVolumeText.text = currentMasterVolume.ToString(); // Update display text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1)) // If main menu scene
                {
                    MusicManager.Instance.audioSource.volume = currentMusicVolume * 0.1f * currentMasterVolume * 0.1f; // Update the volume of the audioSource
                }
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3)) // If main game scene
                {
                    PhysicsCalculations.Instance.masterAudioVolume = currentMasterVolume; // Update the volume of the attribute
                }
                var masterAudio = new Dictionary<string, object> { { "MasterAudio", currentMasterVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(masterAudio); // Save as above
            }
        });
        decreaseMasterVolumeButton.onClick.AddListener(async () =>
        {
            if (currentMasterVolume != 0) // If not yet at 0 volume
            {
                currentMasterVolume--; // Decrement
                masterVolumeText.text = currentMasterVolume.ToString(); // Update display text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1)) // If main menu scene
                {
                    MusicManager.Instance.audioSource.volume = currentMusicVolume * 0.1f * currentMasterVolume * 0.1f; // Update the volume of the audioSource
                }
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3)) // If main game scene
                {
                    PhysicsCalculations.Instance.masterAudioVolume = currentMasterVolume; // Update the volume of the attribute
                }
                var masterAudio = new Dictionary<string, object> { { "MasterAudio", currentMasterVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(masterAudio); // Save as above
            }
        });
        increaseMusicVolumeButton.onClick.AddListener(async () =>
        {
            if (currentMusicVolume != 10) // If not yet at max volume
            {
                currentMusicVolume++; // Increment
                musicVolumeText.text = currentMusicVolume.ToString(); // Update display text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1)) // If main menu scene
                {
                    MusicManager.Instance.audioSource.volume = currentMusicVolume * 0.1f * currentMasterVolume * 0.1f; // Update the volume of the audioSource
                }
                var musicAudio = new Dictionary<string, object> { { "MusicAudio", currentMusicVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(musicAudio); // Save as above
            }
        });
        decreaseMusicVolumeButton.onClick.AddListener(async () =>
        {
            if (currentMusicVolume != 0) // If not yet at 0 volume
            {
                currentMusicVolume--; // Decrement
                musicVolumeText.text = currentMusicVolume.ToString(); // Update text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1)) // If main menu scene
                {
                    MusicManager.Instance.audioSource.volume = currentMusicVolume * 0.1f * currentMasterVolume * 0.1f; // Update the volume of the audioSource
                }
                var musicAudio = new Dictionary<string, object> { { "MusicAudio", currentMusicVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(musicAudio); // Save as above
            }
        });
        increaseSoundEffectVolumeButton.onClick.AddListener(async () =>
        {
            if (currentSoundEffectVolume != 10) // If not yet at max volume
            {
                currentSoundEffectVolume++; // Increment
                soundEffectVolumeText.text = currentSoundEffectVolume.ToString(); // Update text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3)) // If main game scene
                {
                    PhysicsCalculations.Instance.soundEffectAudioVolume = currentSoundEffectVolume; // Update the volume of the attribute
                }
                var soundEffectAudio = new Dictionary<string, object> { { "SoundEffectAudio", currentSoundEffectVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(soundEffectAudio); // Save as above
            }
        });
        decreaseSoundEffectVolumeButton.onClick.AddListener(async () =>
        {
            if (currentSoundEffectVolume != 0) // If not yet at 0 volume
            {
                currentSoundEffectVolume--; // Decrement
                soundEffectVolumeText.text = currentSoundEffectVolume.ToString(); // Update text
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3)) // If main game scene
                {
                    PhysicsCalculations.Instance.soundEffectAudioVolume = currentSoundEffectVolume; // Update the volume of the attribute
                }
                var soundEffectAudio = new Dictionary<string, object> { { "SoundEffectAudio", currentSoundEffectVolume } }; // Create a dictionary as above
                await CloudSaveService.Instance.Data.Player.SaveAsync(soundEffectAudio); // Save as above
            }
        });
    }

    private void Update()
    {
        if (showUI) // If showUI is true
        {
            if (UITransitions.Instance.ShowUI(backgroundImage, containerTransform, previousScale, SETTINGS_UI) == false) // If the transition is complete
            {
                showUI = false; // Set showUI to false
            }
        }
        if (hideUI) // If hideUI is true
        {
            if (UITransitions.Instance.HideUI(backgroundImage, containerTransform, previousScale, SETTINGS_UI) == false) // If the transition is complete
            {
                hideUI = false; // Set hideUI to false
                gameObject.SetActive(false); // Disable the game object
            }
        }
        if (transition) // If transition is true (to transition between 2 pages)
        {
            TransitionToPage(); // Transition between the pages
        }
        if (showRebind) // If a rebind is currently in progress...
        {
            AnimateRebindCanvas(); // Transition in the rebind canvas
        }
        if (hideRebind) // If a rebind has been completed
        {
            AnimateHideRebindCanvas(); // Transition out the rebind canvas
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !hideUI && !showUI)
        {
            hideUI = true;
        }
    }
    public async void Show()
    {
        float musicAudio = 0;
        float masterAudio = 0;
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync(); // Load all data items
        IDeserializable item;
        foreach (var saveData in savedData) // For each item
        {
            item = saveData.Value.Value;
            switch (saveData.Key) // Switch the key
            {
                case "Difficulty": // If the key's named "Difficulty"...
                    if (item.GetAs<string>() == "Simple") // Fetch it's value, and check if its "Simple"
                    {
                        simplePhysicsOutline.gameObject.SetActive(true); // If so, set the outline of "Simple" to active
                        break;
                    }
                    else
                    {
                        realisticPhysicsOutline.gameObject.SetActive(true); // Else set the outline of "Realistic" to active
                        break;
                    }
                case "TerrainHeight": // If the key's named "TerrainHeight"
                    terrainHeightText.text = item.GetAs<string>(); // Update the terrain height text
                    currentTerrainHeight = item.GetAs<int>(); // Set the currentTerrainHeight to this value
                    break;
                case "MasterAudio":
                    masterAudio = item.GetAs<float>() * 0.1f;
                    masterVolumeText.text = item.GetAs<string>(); // As above
                    currentMasterVolume = item.GetAs<int>(); // As above
                    break;
                case "MusicAudio":
                    musicAudio = item.GetAs<float>() * 0.1f;
                    musicVolumeText.text = item.GetAs<string>(); // As above
                    currentMusicVolume = item.GetAs<int>(); // As above
                    break;
                case "SoundEffectAudio":
                    soundEffectVolumeText.text = item.GetAs<string>(); // As above
                    currentSoundEffectVolume = item.GetAs<int>(); // As above
                    break;
                default:
                    break;
            }
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1)) // If main menu scene
        {
            MusicManager.Instance.audioSource.volume = musicAudio * masterAudio; // Update the audioSource volume in the MusicManager
        }
        showUI = true; // When the settings button is pressed, set showUI to true
        gameObject.SetActive(true); // Enable the game object
        containerTransform.localScale = previousScale; // Set the scale to the minimum scale
        UpdateVisual();
    }

    public void Hide()
    {
        hideUI = true; // When the back button is presed, set hideUI to true
    }

    private void PageTransition(GameObject current, GameObject goingTo)
    {
        transition = true; // Set transition to true
        currentPage = current; // Set the current page to the given one
        currentPageCanvasGroup = currentPage.GetComponent<CanvasGroup>(); // Fetch the canvas group of the page
        pageToTransitionTo = goingTo; // Set the page to be transitioned to to the given one
        pageToTransitionToCanvasGroup = pageToTransitionTo.GetComponent<CanvasGroup>(); // Fetch this page's canvas group
        pageToTransitionTo.SetActive(true); // Set the game object of the page to be transitioned to to active
    }

    private void TransitionToPage()
    {
        if (currentPageCanvasGroup.alpha != 0f) // If the alpha is not yet 0
        {
            currentPageCanvasGroup.alpha -= Time.unscaledDeltaTime * 4; // Decrement the alpha
        }
        else if (pageToTransitionToCanvasGroup.alpha != 1f) // If the alpha is not yet 1
        {
            pageToTransitionToCanvasGroup.alpha += Time.unscaledDeltaTime * 4; // Increment the alpha
        }
        else // If currentPage is 0 and pageToTransitionTo is 1
        {
            transition = false; // Set transition to false as the transition is complete
            currentPage.SetActive(false); // Disable the game object of the current page
            currentPage = pageToTransitionTo; // Set the current page to the transitioned to page
        }
    }

    private void UpdateVisual()
    {
        rollLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.rollLeft); // Set the text on each binding button to its respective binding
        rollRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.rollRight);
        throttleUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.throttleUp);
        throttleDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.throttleDown);
        spoilersText.text = GameInput.Instance.GetBindingText(GameInput.Binding.spoilers);
        pitchDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.pitchDown);
        pitchUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.pitchUp);
        flapsEnableText.text = GameInput.Instance.GetBindingText(GameInput.Binding.flapsEnable);
        flapsDisableText.text = GameInput.Instance.GetBindingText(GameInput.Binding.flapsDisable);
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true); // Set the canvas active
        showRebind = true; // Set showRebind to true so that it gets transitioned in from 0 alpha to 1 alpha
    }

    private void HidePressToRebindKey()
    {
        hideRebind = true; // Set hideRebind to true so that it transitions out
    }

    private void RebindBinding(GameInput.Binding binding) // On a rebind being called...
    {
        ShowPressToRebindKey(); // Show the rebind canvas
        GameInput.Instance.RebindBinding(binding, () => // Call the RebindBinding function, passing in the given binding, and the action to be done upon completion
        {
            HidePressToRebindKey(); // Hide the rebind canvas
            UpdateVisual(); // Update the visual text on the rebind buttons
        });
    }

    private void AnimateRebindCanvas()
    {
        if (pressToRebindKeyCanvasGroup.alpha != 1f && hideRebind != true) // If the alpha is not yet 1 and hiding is not true (prevents showing + hiding at same time)
        {
            pressToRebindKeyCanvasGroup.alpha += Time.unscaledDeltaTime * 4; // Increment
        }
        else
        {
            showRebind = false; // Once alpha is 1, set showRebind to false as its done
        }
    }

    private void AnimateHideRebindCanvas()
    {
        if (pressToRebindKeyCanvasGroup.alpha != 0f) // If alpha is not yet 0
        {
            pressToRebindKeyCanvasGroup.alpha -= Time.unscaledDeltaTime * 4; // Decrement
        }
        else
        {
            pressToRebindKeyTransform.gameObject.SetActive(false); // Disable the game object
            hideRebind = false; // Set hideRebind to false as its done
        }
    }
}
