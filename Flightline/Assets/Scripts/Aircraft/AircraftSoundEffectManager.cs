using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AircraftSoundEffectManager : MonoBehaviour
{
    public static AircraftSoundEffectManager Instance {  get; private set; }

    [SerializeField] PhysicsCalculations plane;
    public AudioSource soundEffectAudioSource;

    public AudioClip[] soundEffectChoices;

    private void Start()
    {
        soundEffectAudioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(4)) // If tutorial scene
        {
            soundEffectAudioSource.volume = plane.Throttle * PhysicsCalculations.Instance.soundEffectAudioVolume * 0.1f * PhysicsCalculations.Instance.masterAudioVolume * 0.1f;
        }
        else if (!PauseUI.Instance.pauseUI.activeSelf && !FlightCompletionUI.Instance.flightCompletionScreen.activeSelf)
        {
            soundEffectAudioSource.mute = false;
            soundEffectAudioSource.volume = plane.Throttle * PhysicsCalculations.Instance.soundEffectAudioVolume * 0.1f * PhysicsCalculations.Instance.masterAudioVolume * 0.1f;
        }
        else
        {
            soundEffectAudioSource.mute = true;
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
