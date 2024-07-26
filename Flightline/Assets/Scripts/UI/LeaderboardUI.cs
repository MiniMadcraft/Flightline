using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave.Models;
using Unity.Services.Leaderboards;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance { get; private set; }
    public Button backButton;
    public CanvasGroup backgroundImage;
    public Transform containerTransform;
    private bool showUI;
    private bool hideUI;
    private bool fadeEntryIn;
    private bool fadeEntryOut;
    public Vector3 previousScale = new Vector3(0.2f, 0.2f, 0.2f);
    private const string LEADERBOARD_UI = "LeaderboardUI";

    [Header("Searching Leaderboard")]
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] Button searchButton;
    [SerializeField] Button clearButton;
    [SerializeField] GameObject entryHolder;
    [SerializeField] GameObject noEntryFoundHolder;
    [SerializeField] CanvasGroup entryHolderCanvasGroup;
    [SerializeField] TextMeshProUGUI positionText;
    [SerializeField] TextMeshProUGUI usernameText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Leaderboard Fields")]
    public TextMeshProUGUI[] usernameArray;
    public TextMeshProUGUI[] scoreArray;

    LeaderboardData data;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gameObject.SetActive(false); // Set the game object inactive
        backButton.onClick.AddListener(() => // When the back button is pressed
        {
            hideUI = true; // Set hideUI to true
            usernameInputField.text = "";
            if (entryHolder.activeSelf) // If entry holder active
            {
                fadeEntryOut = true; // Fade out
            }
            if (noEntryFoundHolder.activeSelf) // If no entry found holder active
            {
                noEntryFoundHolder.SetActive(false); // Disable
            }
            usernameInputField.text = "";
        });
        searchButton.onClick.AddListener(() =>
        {
            if (usernameInputField.text != null && !showUI && !hideUI && !fadeEntryIn && !fadeEntryOut) // If no transitions playing and input field is not empty
            {
                SearchForEntry(); // Search for valid entry
            }
        });
        clearButton.onClick.AddListener(() =>
        {
            if (!showUI && !hideUI && !fadeEntryIn && !fadeEntryOut) // If no transitions playing
            {
                if (entryHolder.activeSelf) // If entry holder active
                {
                    fadeEntryOut = true; // Fade out
                    usernameInputField.text = ""; // Reset input field
                }
                else // If not active
                {
                    noEntryFoundHolder.SetActive(false); // Disable no entry found holder
                    usernameInputField.text = "";
                }
            }
        });
        backgroundImage.alpha = 0f; // Initially set the alpha value to 0
    }

    private void Update()
    {
        if (showUI) // If showUI is true
        {
            if (UITransitions.Instance.ShowUI(backgroundImage, containerTransform, previousScale, LEADERBOARD_UI) == false) // If the transition has finished
            {
                showUI = false; // Set showUI to false
            }
        }
        if (hideUI) // If hideUI is true
        {
            if (UITransitions.Instance.HideUI(backgroundImage, containerTransform, previousScale, LEADERBOARD_UI) == false) // If the transition has finished
            {
                hideUI = false; // Set hideUI to false
                gameObject.SetActive(false); // Set the game object inactive
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !hideUI && !showUI)
        {
            hideUI = true;
            if (entryHolder.activeSelf) // If entry holder active
            {
                fadeEntryOut = true; // Fade out
            }
            if (noEntryFoundHolder.activeSelf) // If no entry found holder active
            {
                noEntryFoundHolder.SetActive(false); // Disable
            }
            usernameInputField.text = "";
        }
        if (fadeEntryIn)
        {
            PlayFadeInTransition();
        }
        if (fadeEntryOut)
        {
            PlayFadeOutTransition();
        }
    }
    public async void Show()
    {
        showUI = true; // When the leaderboard button is pressed, set showUI to true
        gameObject.SetActive(true); // Enable the game object
        containerTransform.localScale = previousScale; // Set the scale to the minimum scale
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync( // Load all scores from the "ScoreLeaderboard"
            "ScoreLeaderboard",
            new GetScoresOptions { Limit = 1000});
        data = JsonUtility.FromJson<LeaderboardData>(JsonConvert.SerializeObject(scoreResponse)); // Convert the scoreResponse to a JSON string, then convert it to the LeaderboardData type
        int max = data.results.Count < 10 ? data.results.Count : 10;
        for (int i = 0; i < max; i++) // For each entry in top 10
        {
            usernameArray[i].text = data.results[i].playerName.Remove(data.results[i].playerName.Length - 5); // Set the username at position i to the playerName, minus the last 5 characters
            scoreArray[i].text = data.results[i].score.ToString(); // Set the score at the position i to the score of the user
        }
    }

    public void Hide()
    {
        hideUI = true; // When the back button is pressed, set hideUI to true
    }

    private void SearchForEntry()
    {
        int flag = 0;
        usernameInputField.text = usernameInputField.text.ToLower();
        for (int i = 0; i < data.results.Count; i++) // For each leaderboard entry
        {
            if (usernameInputField.text == data.results[i].playerName.Remove(data.results[i].playerName.Length - 5)) // If given display name equal to display name of entry
            {
                fadeEntryIn = true; // Fade in the entry holder
                entryHolder.SetActive(true);
                positionText.text = "#" + (i + 1); // Set the position to the given position of entry
                usernameText.text = data.results[i].playerName.Remove(data.results[i].playerName.Length - 5); // Set usernameText to given display name minus last 5 characters
                scoreText.text = data.results[i].score.ToString(); // Set scoreText to given score
                noEntryFoundHolder.SetActive(false); // Disable the noEntryFoundHolder in case it was previously active
                flag = 1;
                break; // Exit the for loop
            }
        }
        if (flag != 1) // If the flag is not 1, i.e no valid entry found
        {
            noEntryFoundHolder.SetActive(true); // Enable the noEntryFoundHolder
            if (entryHolder.activeSelf) // If the entryHolder is currently active, i.e searched a valid username now searching an invalid username
            {
                fadeEntryOut = true; // Fade it out
            }
        }
    }

    private void PlayFadeInTransition()
    {
        if (entryHolderCanvasGroup.alpha != 1f) // If alpha value not yet 1
        {
            entryHolderCanvasGroup.alpha += Time.deltaTime * 6; // Increment
        }
        else
        {
            fadeEntryIn = false; // Set bool false
        }
    }

    private void PlayFadeOutTransition()
    {
        if (entryHolderCanvasGroup.alpha != 0f) // If alpha value not yet 0
        {
            entryHolderCanvasGroup.alpha -= Time.deltaTime * 6; // Decrement
        }
        else
        {
            fadeEntryOut = false; // Set bool false
            entryHolder.SetActive(false); // Disable the game object
        }
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string playerId;
    public string playerName;
    public int rank;
    public float score;

    public LeaderboardEntry(string _playerId, string _playerName, int _rank, float _score) // Holds all fields in a leaderboard entry
    {
        playerId = _playerId;
        playerName = _playerName;
        rank = _rank;
        score = _score;
    }
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> results; // Holds the list containing all individual leaderboard entries
}