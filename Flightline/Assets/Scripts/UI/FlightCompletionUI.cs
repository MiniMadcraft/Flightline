using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class FlightCompletionUI : MonoBehaviour
{
    public static FlightCompletionUI Instance {  get; private set; }

    [SerializeField] public GameObject flightCompletionScreen;
    [SerializeField] CanvasGroup flightCompletionScreenCanvasGroup;
    [SerializeField] TextMeshProUGUI flightTimeText;
    [SerializeField] TextMeshProUGUI aircraftText;
    [SerializeField] TextMeshProUGUI landingScoreText;
    [SerializeField] TextMeshProUGUI flightScoreText;
    [SerializeField] TextMeshProUGUI totalScoreText;
    [SerializeField] TextMeshProUGUI flightCompletedText;
    [SerializeField] GameObject staticTextHolder;
    [SerializeField] GameObject updatableTextHolder;
    [SerializeField] GameObject crashedTextHolder;
    [SerializeField] Button okButton;

    private bool fadeIn = false;
    private bool isFirstSave = true;

    private float time;

    private void Start()
    {
        Instance = this;
        okButton.onClick.AddListener(() =>
        {
            BackgroundUIMainGameTransition.Instance.TransitionOut();
        });
    }

    private void Update()
    {
        if (fadeIn)
        {
            FadeInTransition();
        }
        time += Time.deltaTime;
    }
    public async Task UpdateFlightStatisticsAndLeaderboard(int landingScore, int flightScore)
    {
        if (isFirstSave) // If the first time trying to save the score
        {
            isFirstSave = false; // Set false
            int intTime = Mathf.RoundToInt(time); // Fetch time as an int
            int hours = intTime / 3600; // Calculate hours
            int minutes = (intTime - (hours * 3600)) / 60; // Calculate minutes
            int seconds = (intTime - (hours * 3600) - (minutes * 60)); // Calculate seconds
            FlightEntry flightEntry = new FlightEntry(System.DateTime.Now.ToShortDateString(), AircraftLoader.Instance.aircraftSelection, landingScore + flightScore); // Create a new entry with given fields
            aircraftText.text = flightEntry.aircraft;
            totalScoreText.text = flightEntry.score.ToString();
            flightScoreText.text = flightScore.ToString(); // Set the text's to respective strings
            landingScoreText.text = landingScore.ToString();
            flightTimeText.text = hours + "H " + minutes + "M " + seconds + "S";

            await FlightDataHandler.Instance.SaveFlightData(flightEntry); // Save the flight data
            if (PlayerPrefs.GetString("PlayerName") != "Guest") // If not a guest
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync("ScoreLeaderboard", flightEntry.score); // Add score to leaderboard
            }
        }
    }

    public async void SetFadeIn(int givenLandingScore, int givenFlightScore)
    {
        if (givenLandingScore > 5) // If score greater than 5 aka not crashed
        {
            await UpdateFlightStatisticsAndLeaderboard(givenLandingScore, givenFlightScore); // Call the UpdateFlightStatisticsAndLeaderboard function ONCE
        }
        else
        {
            flightCompletedText.text = "crashed"; // User crashed
            staticTextHolder.SetActive(false); // Disable the completion screen box text
            updatableTextHolder.SetActive(false);
            crashedTextHolder.SetActive(true); // Enable the completion screen crashed box text
        }
        fadeIn = true;
        flightCompletionScreen.SetActive(true); // Enable the flight completion screen
    }

    private void FadeInTransition()
    {
        if (flightCompletionScreenCanvasGroup.alpha != 1f) // If alpha not yet 1
        {
            flightCompletionScreenCanvasGroup.alpha += Time.unscaledDeltaTime * 3f; // Increment
        }
        else
        {
            fadeIn = false;
            Time.timeScale = 0f; // Set time scale to 0
        }
    }
}
