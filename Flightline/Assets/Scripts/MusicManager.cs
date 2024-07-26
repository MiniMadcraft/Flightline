using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] Image musicIcon;
    [SerializeField] TextMeshProUGUI songName;
    [SerializeField] TextMeshProUGUI artistName;
    [SerializeField] Image position;
    [SerializeField] TextMeshProUGUI currentAudioPosition;
    [SerializeField] TextMeshProUGUI endAudioPosition;
    private int currentSeconds = 0;
    private int previousSeconds = 0;
    private int endTime;
    private int val;
    public AudioClip[] musicChoices;
    public TextMeshProUGUI[] musicNames;
    public TextMeshProUGUI[] artistNames;
    public AudioSource audioSource;
    private bool isPlaying;
    private Transform positionTransform;
    private float transformDistance;
    private Vector3 startingPosition;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Fetch the AudioSource component
        positionTransform = position.GetComponent<Transform>(); // Fetch the transform
        isPlaying = false; // Initially set isPlaying to false
        startingPosition = new Vector3(positionTransform.localPosition.x, positionTransform.localPosition.y, positionTransform.localPosition.z); // Reset the starting position of the position line
    }

    private void Update()
    {
        Play();
        musicIcon.transform.Rotate(0, 0, -0.5f); // Rotate the music player icon clockwise
    }

    private void Play()
    {
        if (!isPlaying)
        {
            isPlaying = true; // Is now playing music
            System.Random random = new System.Random();
            val = random.Next(musicChoices.Length); // Generate a random track
            audioSource.clip = musicChoices[val]; // Set the audio source to that track
            audioSource.Play(); // Play the track
            endTime = (int)audioSource.clip.length; // Fetch the length of the track
            int minutes = endTime / 60;
            int seconds = endTime - (minutes * 60);
            endAudioPosition.text = minutes + ":" + (seconds < 10 ? "0" + seconds : seconds); // Set the end time to this length
            currentAudioPosition.text = "0:00"; // Set current time to 0:00
            artistName.text = artistNames[val].text;
            songName.text = musicNames[val].text; // Set artist and song name
            transformDistance = 173.024f / endTime; // Find the distance moved per second of the position line when the long line is 173.024 units long
        }
        else
        {
            if (currentSeconds == endTime) // If at end of song
            {
                isPlaying = false; // Set bool to false
                currentSeconds = 0; // Reset current/previous time to 0
                previousSeconds = 0;
                positionTransform.localPosition = new Vector3(startingPosition.x, startingPosition.y, startingPosition.z); // Reset the position of the position line
            }
            else
            {
                currentSeconds = (int)audioSource.time; // Fetch current time through track
                int minutes = currentSeconds / 60;
                int seconds = currentSeconds - (minutes * 60);
                currentAudioPosition.text = minutes + ":" + (seconds < 10 ? "0" + seconds : seconds); // Set current position
                if (previousSeconds != currentSeconds) // If the time has changed
                {
                    positionTransform.Translate(transformDistance, 0, 0); // Adjust the position of the position line transform
                }
                previousSeconds = currentSeconds; // Set previous seconds to the current seconds
            }
        }
    }
}