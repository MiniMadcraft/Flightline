using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using Unity.Services.Leaderboards;

public class MainInterfaceTransition : MonoBehaviour
{
    public static MainInterfaceTransition Instance { get; private set; }
    private Vector3 previousScale = new Vector3(0.5f, 0.5f, 0.5f);
    public GameObject mainInterface;
    public GameObject buttons;
    public CanvasGroup loadingText;
    private CanvasGroup mainInterfaceCanvas;
    private Transform mainInterfaceTransform;
    public CanvasGroup backgroundAircraftCanvas;
    private bool transitionInPlayed = false;
    public bool transitionOutPlaying = false;
    private bool transitionOutPlayed = false;
    private float time;
    public Loader.Scene givenScene;

    private void Awake()
    {
        Instance = this;
    }
    private async void Start()
    {
        mainInterfaceCanvas = mainInterface.GetComponent<CanvasGroup>(); // Fetch the CanvasGroup component
        mainInterfaceTransform = mainInterface.GetComponent<Transform>(); // Fetch the transform component
        mainInterfaceCanvas.alpha = 0f; // Reset the alpha value to 0
        mainInterfaceTransform.localScale = new Vector3(previousScale.x, previousScale.y, previousScale.z); // Reset the scale to the minimum
        loadingText.alpha = 1f;
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync(); // Load all data items
        IDeserializable item;
        float musicAudio = 0;
        float masterAudio = 0;
        foreach (var saveData in savedData) // For each
        {
            item = saveData.Value.Value; // Fetch stored value
            switch (saveData.Key) // Pass in the key
            {
                case "MusicAudio": // If the key is MusicAudio
                    musicAudio = item.GetAs<float>() * 0.1f; // Set the audioSource volume to the given value at the key
                    break;
                case "MasterAudio":
                    masterAudio = item.GetAs<float>() * 0.1f;
                    break;
                default:
                    break;
            }
        }
        MusicManager.Instance.audioSource.volume = musicAudio * masterAudio;
    }

    private void Update()
    {
        if (!transitionInPlayed) // If the transitionIn has not yet been played
        {
            time += Time.deltaTime; // Increment time
            PlayTransition(); // Play the transition
        }
        if (transitionOutPlaying && !transitionOutPlayed) // If the transitionOut is playing but it has not yet fully played
        {
            PlayOutTransition(); // Play the transition
        }
        if (transitionOutPlayed) // If the transitionOut has already been played
        {
            Loader.Load(givenScene); // Load the given scene
        }
    }

    private void PlayTransition()
    {
        if (time > 2f) // If its been more than 1 second since the scene has loaded
        {
            if (mainInterfaceCanvas.alpha != 1f) // If the alpha value is not yet 1
            {
                mainInterfaceCanvas.alpha += (Time.deltaTime * 3f); // Increment the alpha
            }
            if (mainInterfaceTransform.localScale != new Vector3(1f, 1f, 1f)) // If the scale is not yet 1 on all axis
            {
                mainInterfaceTransform.localScale = new Vector3(previousScale.x + 0.02f, previousScale.y + 0.02f, previousScale.z + 0.02f); // Increment the scale
            }
            if (loadingText.alpha != 0f) // If alpha is not yet 0
            {
                loadingText.alpha -= Time.deltaTime * 4; // Decrement alpha
            }
            if (mainInterfaceCanvas.alpha == 1f && mainInterfaceTransform.localScale == new Vector3(1f, 1f, 1f) && loadingText.alpha == 0f) // If main alpha is 1 and scale is 1 on all axis and loading alpha is 0
            {
                transitionInPlayed = true; // Set the transitionInPlayed attribute to true
                buttons.SetActive(true);
            }
            previousScale = mainInterfaceTransform.localScale; // Set the previousScale to this new scale
        }
    }

    private void PlayOutTransition()
    {
        if (mainInterfaceCanvas.alpha != 0f) // If the alpha value is not yet 0
        {
            mainInterfaceCanvas.alpha -= (Time.deltaTime * 3f); // Decrement the alpha value
        }
        if (backgroundAircraftCanvas.alpha != 0f) // If the alpha value is not yet 0
        {
            backgroundAircraftCanvas.alpha -= (Time.deltaTime * 3f); // Decrement the alpha value
        }
        if (loadingText.alpha != 1f)
        {
            loadingText.alpha += (Time.deltaTime * 2f);
        }
        if (MusicManager.Instance.audioSource.volume != 0f) // If the audio volume is not yet 0
        {
            MusicManager.Instance.audioSource.volume -= Time.deltaTime; // Decrement the volume
        }
        if (mainInterfaceCanvas.alpha == 0f && MusicManager.Instance.audioSource.volume == 0f && backgroundAircraftCanvas.alpha == 0f && loadingText.alpha == 1f) // If all 3 conditions are now met
        {
            transitionOutPlaying = false; // Set the playing attribute to false
            transitionOutPlayed = true; // Set the played attribute to true
        }
    }

    public void SetOutTransition(Loader.Scene sceneToTransitionTo)
    {
        transitionOutPlaying = true; // When the play button is pressed, set the transitionOutPlaying attribute to true
        givenScene = sceneToTransitionTo;
    }
}
