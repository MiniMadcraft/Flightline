using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Newtonsoft.Json;

public class ProfileUI : MonoBehaviour
{
    public static ProfileUI Instance { get; private set; }
    public Button backButton;
    public CanvasGroup backgroundImage;
    public Transform containerTransform;
    private bool showUI;
    private bool hideUI;
    public Vector3 previousScale = new Vector3(0.2f, 0.2f, 0.2f);
    private const string PROFILE_UI = "ProfileUI";

    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI IDText;
    public TextMeshProUGUI userSinceText;

    LeaderboardData data;

    [Header("Update Username and Password")]
    [SerializeField] private GameObject updateDisplayNameHolder;
    [SerializeField] private GameObject updatePasswordHolder;
    [SerializeField] private GameObject statisticsAndUpdateButtonsHolder;
    [SerializeField] private GameObject errorPage;
    [SerializeField] private CanvasGroup updateDisplayNameHolderCanvasGroup;
    [SerializeField] private CanvasGroup updatePasswordHolderCanvasGroup;
    [SerializeField] private CanvasGroup statisticsAndUpdateButtonsHolderCanvasGroup;
    [SerializeField] private CanvasGroup errorPageCanvasGroup;
    [SerializeField] private Button updateDisplayNameButton;
    [SerializeField] private Button updatePasswordButton;
    [SerializeField] private Button submitDisplayNameButton;
    [SerializeField] private Button submitPasswordButton;
    [SerializeField] private Button displayNamePageBackButton;
    [SerializeField] private Button passwordPageBackButton;
    [SerializeField] private Button okButton;
    [SerializeField] private TMP_InputField displayNameInputField;
    [SerializeField] private TMP_InputField oldPasswordInputField;
    [SerializeField] private TMP_InputField newPasswordInputField;
    [SerializeField] private Button oldVisibilityButton;
    [SerializeField] private Button newVisibilityButton;
    [SerializeField] private Image oldOpenEyeImage;
    [SerializeField] private Image newOpenEyeImage;
    [SerializeField] private Image oldClosedEyeImage;
    [SerializeField] private Image newClosedEyeImage;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI errorHeaderText;

    [Header("Convert Guest Account")]
    [SerializeField] private GameObject createAccountButtonHolder;
    [SerializeField] private GameObject convertAccountHolder;
    [SerializeField] private GameObject updateButtons;
    [SerializeField] private CanvasGroup createAccountButtonHolderCanvasGroup;
    [SerializeField] private CanvasGroup convertAccountHolderCanvasGroup;
    [SerializeField] private Button chooseCreateAccountButton;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button createAccountPasswordVisibilityButton;
    [SerializeField] private Button createAccountBackButton;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Image createAccountOpenEyeImage;
    [SerializeField] private Image createAccountClosedEyeImage;

    [Header("Previous Flights")]
    public TextMeshProUGUI[] dates;
    public TextMeshProUGUI[] aircraft;
    public TextMeshProUGUI[] scores;

    [Header("Statistics")]
    [SerializeField] TextMeshProUGUI totalFlightsText;
    [SerializeField] TextMeshProUGUI totalScoreText;

    private GameObject previousPage;
    private CanvasGroup previousPageCanvasGroup;
    private GameObject nextPage;
    private CanvasGroup nextPageCanvasGroup;

    private bool transition = false;
    private bool isOldPasswordVisible = false;
    private bool isNewPasswordVisible = false;
    private bool isCreateAccountPasswordVisible = false;
    private bool showErrorPage = false;
    private bool hideErrorPage = false;
    private bool successfulAccountCreation = false;
    private bool statisticsFadeIn = false;
    private bool statisticsFadeOut = false;

    private bool firstCycle = true;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gameObject.SetActive(false); // Initially set the game object inactive
        backButton.onClick.AddListener(() => // When the back button is clicked
        {
            hideUI = true; // Set hideUI to true
            if (previousPage != statisticsAndUpdateButtonsHolder && usernameText.text != "guest") // If not currently on the default page and not a guest
            {
                displayNameInputField.text = ""; // Reset input fields
                oldPasswordInputField.text = "";
                newPasswordInputField.text = "";
                nextPage = statisticsAndUpdateButtonsHolder;
                nextPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
                nextPage.SetActive(true); // Transition between pages
                transition = true;
            }
            if (previousPage != statisticsAndUpdateButtonsHolder && usernameText.text == "guest")
            {
                transition = true;
                nextPage = createAccountButtonHolder;
                nextPageCanvasGroup = createAccountButtonHolderCanvasGroup; // Transition to the create account button page
                nextPage.SetActive(true);
                statisticsFadeIn = true;
                statisticsAndUpdateButtonsHolder.SetActive(true);
                usernameField.text = "";
                passwordField.text = "";
            }
            firstCycle = true;
        });
        updateDisplayNameButton.onClick.AddListener(() => // On click
        {
            if (!transition) // If not currently playing a transition
            {
                transition = true; // Set transition to true
                nextPage = updateDisplayNameHolder; // Set the next page to the given page
                nextPageCanvasGroup = updateDisplayNameHolderCanvasGroup; // Set the next page canvas group to the given one
                nextPage.SetActive(true); // Set the "next page" active
            }
        });
        updatePasswordButton.onClick.AddListener(() => // As above
        {
            if (!transition)
            {
                transition = true;
                nextPage = updatePasswordHolder;
                nextPageCanvasGroup = updatePasswordHolderCanvasGroup;
                nextPage.SetActive(true);
            }
        });
        displayNamePageBackButton.onClick.AddListener(() => // As above
        {
            if (!transition)
            {
                transition = true;
                nextPage = statisticsAndUpdateButtonsHolder;
                nextPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
                nextPage.SetActive(true);
                displayNameInputField.text = "";
            }
        });
        passwordPageBackButton.onClick.AddListener(() => // As above
        {
            if (!transition)
            {
                transition = true;
                nextPage = statisticsAndUpdateButtonsHolder;
                nextPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
                nextPage.SetActive(true);
                oldPasswordInputField.text = "";
                newPasswordInputField.text = "";
            }
        });
        submitDisplayNameButton.onClick.AddListener(async () => 
        {
            await CheckUsernameAsync(displayNameInputField.text); // Update the display name of the user with the given display name text
        });
        submitPasswordButton.onClick.AddListener(async () =>
        {
            await UpdatePasswordAsync(oldPasswordInputField.text, newPasswordInputField.text); // Update the user's password
        });
        oldVisibilityButton.onClick.AddListener(() =>
        {
            isOldPasswordVisible = !isOldPasswordVisible; // Invert the bool
            if (isOldPasswordVisible)
            {
                oldPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
                oldPasswordInputField.textComponent.SetAllDirty();
            }
            else
            {
                oldPasswordInputField.contentType = TMP_InputField.ContentType.Password;
                oldPasswordInputField.textComponent.SetAllDirty();
            }
            oldOpenEyeImage.gameObject.SetActive(!oldOpenEyeImage.IsActive()); // Set the active status to the opposite of current
            oldClosedEyeImage.gameObject.SetActive(!oldClosedEyeImage.IsActive()); // Set the active status to the opposite of current
        });
        newVisibilityButton.onClick.AddListener(() => // As above
        {
            isNewPasswordVisible = !isNewPasswordVisible;
            if (isNewPasswordVisible)
            {
                newPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
                newPasswordInputField.textComponent.SetAllDirty();
            }
            else
            {
                newPasswordInputField.contentType = TMP_InputField.ContentType.Password;
                newPasswordInputField.textComponent.SetAllDirty();
            }
            newOpenEyeImage.gameObject.SetActive(!newOpenEyeImage.IsActive());
            newClosedEyeImage.gameObject.SetActive(!newClosedEyeImage.IsActive());
        });
        okButton.onClick.AddListener(() => // On clicking the ok button
        {
            hideErrorPage = true; // Set hideErrorPage to true as the user has acknowledged the error in sign in/sign up
            if (successfulAccountCreation) // If a guest account has been successfully converted
            {
                hideUI = true;
                AuthenticationService.Instance.SignOut(); // Sign out
                AuthenticationService.Instance.ClearSessionToken();
                MainInterfaceTransition.Instance.givenScene = Loader.Scene.AuthenticationScene; // Transition to the authentication screen to sign back in
                MainInterfaceTransition.Instance.transitionOutPlaying = true;
            }
        });

        // Guest account creation
        
        if (PlayerPrefs.GetString("PlayerName") != "Guest") // If the account has a username
        {
            previousPage = statisticsAndUpdateButtonsHolder; // Set the previous page to the statistics page as not a guest
            previousPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
            previousPage.SetActive(true);
            previousPageCanvasGroup.alpha = 1.0f;
        }
        else
        {
            previousPage = createAccountButtonHolder;
            previousPageCanvasGroup = createAccountButtonHolderCanvasGroup; // Set the previous page to the create account button page
            previousPage.SetActive(true);
            previousPageCanvasGroup.alpha = 1.0f;
            statisticsAndUpdateButtonsHolder.SetActive(true); // But also enable the game object for the statistics page
            statisticsAndUpdateButtonsHolderCanvasGroup.alpha = 1.0f;
            updateButtons.SetActive(false); // BUT disable the buttons for the updating display name/password as guests should only see the statistics part
        }
        chooseCreateAccountButton.onClick.AddListener(() =>
        {
            if (!transition && !statisticsFadeOut) // If not currently transitioning
            {
                transition = true;
                nextPage = convertAccountHolder;
                nextPageCanvasGroup = convertAccountHolderCanvasGroup; // Transition to the account creation page
                nextPage.SetActive(true);
                statisticsFadeOut = true;
            }
        });
        createAccountBackButton.onClick.AddListener(() =>
        {
            if (!transition && !statisticsFadeIn) // If not currently transitioning
            {
                transition = true;
                nextPage = createAccountButtonHolder;
                nextPageCanvasGroup = createAccountButtonHolderCanvasGroup; // Transition to the create account button page
                nextPage.SetActive(true);
                statisticsFadeIn = true;
                statisticsAndUpdateButtonsHolder.SetActive(true);
                usernameField.text = "";
                passwordField.text = "";
            }
        });
        createAccountPasswordVisibilityButton.onClick.AddListener(() =>
        {
            isCreateAccountPasswordVisible = !isCreateAccountPasswordVisible;
            if (isCreateAccountPasswordVisible) // If password field should be visible
            {
                passwordField.contentType = TMP_InputField.ContentType.Standard; // Set type to standard
                passwordField.textComponent.SetAllDirty(); // Force text to regenerate
            }
            else
            {
                passwordField.contentType = TMP_InputField.ContentType.Password; // Set the type to password
                passwordField.textComponent.SetAllDirty(); // Force the text to regenerate
            }
            createAccountOpenEyeImage.gameObject.SetActive(!createAccountOpenEyeImage.IsActive()); // Set active state of open and closed eyes to opposite of current
            createAccountClosedEyeImage.gameObject.SetActive(!createAccountClosedEyeImage.IsActive());
        });
        createAccountButton.onClick.AddListener(async () =>
        {
            await ConvertGuestAccount(usernameField.text, passwordField.text); // Attempt to create an account
        });
        backgroundImage.alpha = 0f; // Reset the alpha value of the background to 0
    }

    private void Update()
    {
        if (showUI) // If showUI is true
        {
            if (UITransitions.Instance.ShowUI(backgroundImage, containerTransform, previousScale, PROFILE_UI) == false) // If the transition is complete
            {
                showUI = false; // Set showUI to false
            }
        }
        if (hideUI) // If hideUI is true
        {
            if (UITransitions.Instance.HideUI(backgroundImage, containerTransform, previousScale, PROFILE_UI) == false) // If the transition is complete
            {
                hideUI = false; // Set hideUI to false
                gameObject.SetActive(false); // Disable the game object
            }
        }
        if (transition)
        {
            PlayPageTransitions();
        }
        if (statisticsFadeIn || statisticsFadeOut)
        {
            PlayStatisticsTransitions();
        }
        if (showErrorPage && errorPage.gameObject.activeSelf) // If an error in signing in/signing up + ensuring game object is active before incrementing alpha
        {
            FadeInErrorPage(); // Play the transition
        }
        if (hideErrorPage && !showErrorPage) // If the user has acknowledged the error page
        {
            FadeOutErrorPage(); // Play the transition
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !hideUI && !showUI)
        {
            if (previousPage != statisticsAndUpdateButtonsHolder && usernameText.text != "guest") // If not currently on the default page and not a guest
            {
                displayNameInputField.text = ""; // Reset input fields
                oldPasswordInputField.text = "";
                newPasswordInputField.text = "";
                nextPage = statisticsAndUpdateButtonsHolder;
                nextPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
                nextPage.SetActive(true); // Transition between pages
                transition = true;
            }
            if (previousPage != createAccountButtonHolder && usernameText.text == "guest")
            {
                transition = true;
                nextPage = createAccountButtonHolder;
                nextPageCanvasGroup = createAccountButtonHolderCanvasGroup; // Transition to the create account button page
                nextPage.SetActive(true);
                statisticsFadeIn = true;
                statisticsAndUpdateButtonsHolder.SetActive(true);
                usernameField.text = "";
                passwordField.text = "";
            }
            hideUI = true;
            firstCycle = true;
        }
    }
    public async void Show()
    {
        showUI = true; // When the profileUI button is pressed, set showUI to true
        gameObject.SetActive(true); // Enable the game object
        UpdateRecentFlights();
        containerTransform.localScale = previousScale; // Reset the scale to the minimum value
        if (usernameText.text == "" && firstCycle)
        {
            firstCycle = false;
            Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "AccountCreationDate" });
            IDeserializable item;
            item = savedData["AccountCreationDate"].Value;
            userSinceText.text = "User since " + item.GetAs<string>();
            if (usernameText.text != "guest") // If there is a username, i.e the user is not a guest
            {
                usernameText.text = MainMenuUsernameHandler.Instance.usernameText.text; // Set the username text to their display name minus the last 5 characters
                IDText.text = "User ID: " + AuthenticationService.Instance.PlayerId; // Set the IDText to the user's unique ID
            }
            else // If the user is a guest
            {
                usernameText.text = "Guest"; // Set the username to "Guest"
                IDText.text = "Guest ID: " + AuthenticationService.Instance.PlayerId; // Set the IDText to the Guest's unique ID
            }
        }
    }

    public void Hide()
    {
        hideUI = true; // When the back button is pressed, set hideUI to true
    }

    private void PlayPageTransitions()
    {
        if (previousPageCanvasGroup.alpha != 0f) // If the previous page is not yet 0 alpha
        {
            previousPageCanvasGroup.alpha -= Time.deltaTime * 6; // Decrement
        }
        if (nextPageCanvasGroup.alpha != 1f) // If the next page is not yet 1 alpha
        {
            nextPageCanvasGroup.alpha += Time.deltaTime * 6; // Increment
        }
        if (previousPageCanvasGroup.alpha == 0f && nextPageCanvasGroup.alpha == 1f) // If both at required alpha's
        {
            previousPage.gameObject.SetActive(false); // Set previous page inactive
            previousPage = nextPage; // Set the previousPage to the page just transitioned to
            previousPageCanvasGroup = nextPageCanvasGroup; // Same for canvas group
            transition = false; // Set transition to false as finished
        }
    }

    private void PlayStatisticsTransitions()
    {
        if (statisticsFadeIn) // If fading in statistics page
        {
            if (statisticsAndUpdateButtonsHolderCanvasGroup.alpha != 1f) // If alpha not yet 1
            {
                statisticsAndUpdateButtonsHolderCanvasGroup.alpha += Time.deltaTime * 6; // Increment
            }
            else
            {
                statisticsFadeIn = false;
            }
        }
        if (statisticsFadeOut)
        {
            if (statisticsAndUpdateButtonsHolderCanvasGroup.alpha != 0f) // If alpha not yet 0
            {
                statisticsAndUpdateButtonsHolderCanvasGroup.alpha -= Time.deltaTime * 6; // Decrement
            }
            else
            {
                statisticsFadeOut = false;
                statisticsAndUpdateButtonsHolder.SetActive(false);
            }
        }
    }
    async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            hideUI = true;
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();
            MainInterfaceTransition.Instance.givenScene = Loader.Scene.AuthenticationScene;
            MainInterfaceTransition.Instance.transitionOutPlaying = true;
            Debug.Log("Password updated.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            errorText.text = "invalid password entered\n\nplease ensure that your previous password is correct, and the new password entered meets the criteria of:\n\n8-30 characters\natleast 1 upper and lower case character\natleast 1 number and 1 symbol";
            return;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            errorText.text = "invalid password entered\n\nplease ensure that your previous password is correct, and the new password entered meets the criteria of:\n\n8-30 characters\natleast 1 upper and lower case character\natleast 1 number and 1 symbol";
            return;
        }
    }

    async Task CheckUsernameAsync(string newUsername)
    {
        try
        {
            await CheckDisplayName(newUsername);
            await UpdateUsernameAsync(newUsername);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            errorText.text = "display name already taken\n\nplease ensure that the display name entered meets the criteria of:\n\n1-50 characters\nno white space";
            return;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            errorText.text = "display name already taken\n\nplease ensure that the display name entered meets the criteria of:\n\n1-50 characters\nno white space";
            return;
        }
    }

    private void FadeInErrorPage()
    {
        if (errorPageCanvasGroup.alpha != 1f) // If not yet at 1 alpha
        {
            errorPageCanvasGroup.alpha += Time.deltaTime * 4; // Increment
        }
        else
        {
            showErrorPage = false; // At 1 alpha so set bool false
        }
    }

    private void FadeOutErrorPage()
    {
        if (errorPageCanvasGroup.alpha != 0f) // If not yet at 0 alpha
        {
            errorPageCanvasGroup.alpha -= Time.deltaTime * 4; // Decrement
        }
        else
        {
            hideErrorPage = false; // Set bool false
            errorPage.gameObject.SetActive(false); // Disable game object
        }
    }

    public void ErrorPage()
    {
        errorPage.gameObject.SetActive(true); // Set the error page active
        showErrorPage = true; // Set the bool to true to play the transition
    }

    private async Task ConvertGuestAccount(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.AddUsernamePasswordAsync(usernameField.text, passwordField.text); // Try converting account
            await AuthenticationService.Instance.UpdatePlayerNameAsync(AuthenticationService.Instance.PlayerInfo.Username); // Try setting player name to this name
            await LeaderboardsService.Instance.AddPlayerScoreAsync("ScoreLeaderboard", FetchTotalScore());
            Debug.Log("SignUp is successful."); // If successful, log
            ErrorPage();
            errorHeaderText.text = "success"; // Set error page text messages
            errorText.text = "\nsuccessful account creation\n\nplease now log back in to your account using the details you just provided";
            successfulAccountCreation = true; // Set bool to true so that "Ok" button listener knows to transition to authentication screen
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error"; // Display error messages to user
            errorText.text = "invalid details entered\n\nusername: not case sensitive, must be between 3-20 characters and only contain letters, numbers and symbols\n\npassword: case sensitive, must be between 8-30 characters long, and contain at least 1 upper and lower case character, 1 number, and 1 symbol";
            return;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error"; // Display error messages to user
            errorText.text = "invalid details entered\n\nusername: not case sensitive, must be between 3-20 characters and only contain letters, numbers and symbols\n\npassword: case sensitive, must be between 8-30 characters long, and contain at least 1 upper and lower case character, 1 number, and 1 symbol";
            return;
        }
    }

    async Task CheckDisplayName(string newUsername)
    {
        var scoreResponse = await LeaderboardsService.Instance.GetScoresAsync( // Load all scores from the "ScoreLeaderboard"
                "ScoreLeaderboard");
        data = JsonUtility.FromJson<LeaderboardData>(JsonConvert.SerializeObject(scoreResponse)); // Convert the scoreResponse to a JSON string, then convert it to the LeaderboardData type
        for (int i = 0; i < data.results.Count; i++) // For each entry
        {
            if (data.results[i].playerName.Remove(data.results[i].playerName.Length - 5) == newUsername)
            {
                throw new RequestFailedException(10000, "Username already taken");
            }
        }
    }

    async Task UpdateUsernameAsync(string newUsername)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newUsername);
            Debug.Log("Username updated.");
            PlayerPrefs.SetString("PlayerName", newUsername);
            PlayerPrefs.Save();
            ErrorPage();
            errorHeaderText.text = "success";
            errorText.text = "\ndisplay name successfully updated\n\n note: your display name is separate\nto your username\n\nplease continue using your username, " + AuthenticationService.Instance.PlayerInfo.Username + ", to sign in";
            usernameText.text = newUsername; // Remove last 5 characters of display name
            MainMenuUsernameHandler.Instance.usernameText.text = usernameText.text;
            MainMenuUsernameHandler.Instance.UpdateWelcomeText();
            transition = true;
            nextPage = statisticsAndUpdateButtonsHolder;
            nextPageCanvasGroup = statisticsAndUpdateButtonsHolderCanvasGroup;
            nextPage.SetActive(true);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            errorText.text = "invalid display name entered\n\nplease ensure that the display name entered meets the criteria of:\n\n1-50 characters\nno white space";
            return;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorPage();
            errorHeaderText.text = "error";
            if (ex.ErrorCode == 50)
            {
                errorText.text = "timed out\n\n you have changed your display name too many times in a short period of time\n\n please wait 1 minute before attempting to change your name";
                return;
            }
            else
            {
                errorText.text = "invalid display name entered\n\nplease ensure that the display name entered meets the criteria of:\n\n1-50 characters\nno white space";
                return;
            }
        }
    }

    private void UpdateRecentFlights()
    {
        FlightData flightData = FlightDataHandler.Instance.flightData;
        int max = flightData.results.Count < 10 ? flightData.results.Count : 10; // Find out if there are less than 10 entries or greater than 10 and set max respectively
        int totalFlights = flightData.results.Count;
        int totalScore = 0;
        if (max > 0) // If there are entries
        {
            int x = 0;
            for (int i = totalFlights - 1; i >= totalFlights - max; i--) // From end of list to either total - 10 or total - count if < 10 flights stored
            {
                dates[x].text = flightData.results[i].date; // Set date
                aircraft[x].text = flightData.results[i].aircraft; // Set aircraft
                scores[x].text = flightData.results[i].score.ToString(); // Set score
                x++; // Increment x
            }
            for (int i = 0; i < totalFlights; i++)
            {
                totalScore += flightData.results[i].score; // Increment score
            }
        }
        totalFlightsText.text = "total flights: " + totalFlights.ToString(); // Set statistics
        totalScoreText.text = "total score: " + totalScore.ToString();
    }

    private int FetchTotalScore()
    {
        FlightData flightData = FlightDataHandler.Instance.flightData;
        int max = flightData.results.Count < 10 ? flightData.results.Count : 10; // Find out if there are less than 10 entries or greater than 10 and set max respectively
        int totalFlights = flightData.results.Count;
        int totalScore = 0;
        if (max > 0) // If there are entries
        {
            for (int i = 0; i < totalFlights; i++)
            {
                totalScore += flightData.results[i].score; // Increment score
            }
        }
        return totalScore;
    }
}