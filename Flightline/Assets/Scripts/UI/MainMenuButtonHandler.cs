using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonHandler : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button aircraftSelectionButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button signOutButton;

    private void Start()
    {
        settingsButton.onClick.AddListener(() => { // When the settingsButton is pressed, show the menu
            SettingsUI.Instance.Show();
        });
        leaderboardButton.onClick.AddListener(() => { // As above
            LeaderboardUI.Instance.Show();
        });
        profileButton.onClick.AddListener(() => { // As above
            ProfileUI.Instance.Show();
        });
        aircraftSelectionButton.onClick.AddListener(() => { // As above
            AircraftSelectionUI.Instance.Show();
        });
        playButton.onClick.AddListener(() => // When the playButton is pressed, play the transition
        {
            AircraftSelectionUI.Instance.ShowConfirmationPage(); // Show the confirmation page
        });
        tutorialButton.onClick.AddListener(() =>
        {
            MainInterfaceTransition.Instance.SetOutTransition(Loader.Scene.TutorialScene);
        });
        quitButton.onClick.AddListener(() => // When the quit button is pressed, quit the application
        {
            Application.Quit();
        });
        signOutButton.onClick.AddListener(() => // When the sign out button is pressed...
        {
            AuthenticationService.Instance.SignOut(); // Sign out the user
            MainInterfaceTransition.Instance.SetOutTransition(Loader.Scene.AuthenticationScene); // Transition to the authentication scene
        });
    }

}
