using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave.Internal.Http;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class AircraftSelectionUI : MonoBehaviour
{
    public static AircraftSelectionUI Instance { get; private set; }
    public Button backButton;
    public CanvasGroup backgroundImage;
    public Transform containerTransform;
    private bool showUI;
    private bool hideUI;
    public Vector3 previousScale = new Vector3(0.2f, 0.2f, 0.2f);
    private const string AIRCRAFT_SELECTION_UI = "AircraftSelectionUI";
    private string aircraftSelection;

    [Header("Aircraft Buttons and Outlines")]
    [SerializeField] private Button typhoonButton;
    [SerializeField] private Button eagleButton;
    [SerializeField] private Button businessJetButton;
    [SerializeField] private Button rafaleButton;
    [SerializeField] private GameObject typhoonOutline;
    [SerializeField] private GameObject eagleOutline;
    [SerializeField] private GameObject businessJetOutline;
    [SerializeField] private GameObject rafaleOutline;
    private GameObject currentOutline;

    [Header("Search Page / Aircraft Selection Page")]
    [SerializeField] private GameObject aircraftSelectionPage;
    [SerializeField] private GameObject aircraftSearchPage;
    [SerializeField] private GameObject searchedAircraftHolder;
    [SerializeField] private GameObject searchedAircraftSelectedOutline;
    [SerializeField] private GameObject notYetImplementedObject;
    [SerializeField] private CanvasGroup aircraftSelectionPageCanvasGroup;
    [SerializeField] private CanvasGroup aircraftSearchPageCanvasGroup;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button searchedAircraftButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private TextMeshProUGUI searchedAircraftText;
    [SerializeField] private GameObject invalidAircraftText;

    [Header("Play")]
    [SerializeField] private GameObject confirmButtonHolder;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI aircraftSelectionHeaderText;

    private GameObject previousPage;
    private CanvasGroup previousPageCanvasGroup;
    private GameObject nextPage;
    private CanvasGroup nextPageCanvasGroup;

    private bool transition = false;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        gameObject.SetActive(false); // Initially set the game object inactive
        backButton.onClick.AddListener(() => // If the back button has been clicked...
        {
            if (previousPage != aircraftSelectionPage)
            {
                searchedAircraftHolder.SetActive(false);
                searchedAircraftSelectedOutline.SetActive(false); // Disable holder and outline
                nextPage = aircraftSelectionPage;
                nextPageCanvasGroup = aircraftSelectionPageCanvasGroup;
                nextPage.SetActive(true); // Transition between pages
                transition = true;
            }
            hideUI = true; // Set hideUI to true
            inputField.text = ""; // Reset the input field
        });
        typhoonButton.onClick.AddListener(async () =>
        {
            if (currentOutline != typhoonOutline) // If current outline is not this outline
            {
                currentOutline.SetActive(false); // Disable
                var givenAircraftSelection = new Dictionary<string, object> { { "AircraftSelection", "Typhoon" } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(givenAircraftSelection); // Update the aircraft selection to this aircraft
                currentOutline = typhoonOutline;
                currentOutline.SetActive(true); // Enable
                aircraftSelection = "Typhoon";
            }
        });
        eagleButton.onClick.AddListener(async () =>
        {
            if (currentOutline != eagleOutline) // If current outline is not this outline
            {
                currentOutline.SetActive(false); // Disable
                var givenAircraftSelection = new Dictionary<string, object> { { "AircraftSelection", "Eagle" } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(givenAircraftSelection); // Update the aircraft selection to this aircraft
                currentOutline = eagleOutline;
                currentOutline.SetActive(true); // Enable
                aircraftSelection = "Eagle";
            }
        });
        businessJetButton.onClick.AddListener(async () =>
        {
            if (currentOutline != businessJetOutline) // If current outline is not this outline
            {
                currentOutline.SetActive(false); // Disable
                var givenAircraftSelection = new Dictionary<string, object> { { "AircraftSelection", "Business Jet" } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(givenAircraftSelection); // Update the aircraft selection to this aircraft
                currentOutline = businessJetOutline;
                currentOutline.SetActive(true); // Enable
                aircraftSelection = "Business Jet";
            }
        });
        rafaleButton.onClick.AddListener(async () =>
        {
            if (currentOutline != rafaleOutline) // If current outline is not this outline
            {
                currentOutline.SetActive(false); // Disable
                var givenAircraftSelection = new Dictionary<string, object> { { "AircraftSelection", "Rafale" } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(givenAircraftSelection); // Update the aircraft selection to this aircraft
                currentOutline = rafaleOutline;
                currentOutline.SetActive(true); // Enable
                aircraftSelection = "Rafale";
            }
        });
        searchButton.onClick.AddListener(() =>
        {
            string inputText = inputField.text; // Fetch input text
            if (inputText != "") // If not null
            {
                searchedAircraftSelectedOutline.SetActive(false); // Disable outline
                invalidAircraftText.SetActive(false); // Disable the invalid text
                if (previousPage != aircraftSearchPage) // If currently not on the search page
                {
                    nextPage = aircraftSearchPage;
                    nextPageCanvasGroup = aircraftSearchPageCanvasGroup;
                    transition = true;
                    nextPage.SetActive(true); // Transition between the pages
                }
                if (CheckInputText(inputText)) // If a valid input
                {
                    searchedAircraftHolder.SetActive(true); // Enable the game object
                    if (aircraftSelection == searchedAircraftText.text) // If current aircraft selection is this aircraft
                    {
                        searchedAircraftSelectedOutline.SetActive(true); // Enable the outline
                    }
                }
                if (!CheckInputText(inputText)) // If not a valid input
                {
                    searchedAircraftHolder.SetActive(false); // Disable holder and outline
                    searchedAircraftSelectedOutline.SetActive(false);
                    invalidAircraftText.SetActive(true); // Enable the text to show no valid aircraft found
                }
            }
        });
        clearButton.onClick.AddListener(() =>
        {
            searchedAircraftHolder.SetActive(false);
            searchedAircraftSelectedOutline.SetActive(false); // Disable holder and outline
            nextPage = aircraftSelectionPage;
            nextPageCanvasGroup = aircraftSelectionPageCanvasGroup;
            nextPage.SetActive(true); // Transition between pages
            inputField.text = ""; // Reset input field
            transition = true;
        });
        searchedAircraftButton.onClick.AddListener(async () =>
        {
            if (aircraftSelection != searchedAircraftText.text) // If searched aircraft not the users current aircraft
            {
                currentOutline.SetActive(false); // Disable
                var givenAircraftSelection = new Dictionary<string, object> { { "AircraftSelection", searchedAircraftText.text } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(givenAircraftSelection); // Update the aircraft selection to this aircraft
                aircraftSelection = searchedAircraftText.text; // Set aircraftSelection to this new aircraft
                searchedAircraftSelectedOutline.SetActive(true); // Enable the outline
                InitialAircraftSelectionDisplay(); // Update the outline on the main page
            }
        });
        confirmButton.onClick.AddListener(() =>
        {
            hideUI = true; // Set hideUI to true
            inputField.text = "";
            MainInterfaceTransition.Instance.SetOutTransition(Loader.Scene.MainGameScene);
        });
        backgroundImage.alpha = 0f; // Reset the alpha value of the background image to 0
        previousPage = aircraftSelectionPage;
        previousPageCanvasGroup = aircraftSelectionPageCanvasGroup;
    }

    private void Update()
    {
        if (showUI) // If showUI is true
        {
            if (UITransitions.Instance.ShowUI(backgroundImage, containerTransform, previousScale, AIRCRAFT_SELECTION_UI) == false) // Is the animation complete
            {
                showUI = false; // If so, set showUI to false
            }
        }
        if (hideUI) // If hideUI is true
        {
            if (UITransitions.Instance.HideUI(backgroundImage, containerTransform, previousScale, AIRCRAFT_SELECTION_UI) == false && transition == false) // Is the animation complete
            {
                hideUI = false; // If so, set hideUI to false
                gameObject.SetActive(false); // Disable the game object
            }
        }
        if (transition)
        {
            PlayPageTransitions();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !hideUI && !showUI) // If the esc key is pressed
        {
            if (previousPage != aircraftSelectionPage) // If currently not on the aircraft selection page
            {
                searchedAircraftHolder.SetActive(false);
                searchedAircraftSelectedOutline.SetActive(false); // Disable holder and outline
                nextPage = aircraftSelectionPage;
                nextPageCanvasGroup = aircraftSelectionPageCanvasGroup;
                nextPage.SetActive(true); // Transition between pages
                transition = true;
            }
            hideUI = true;
            inputField.text = ""; // Reset the input field
        }
    }
    public async void Show()
    {
        showUI = true; // Set showUI to true when the aircraftSelection button is pressed
        gameObject.SetActive(true); // Enable the game object
        aircraftSelectionHeaderText.text = "aircraft selection";
        confirmButtonHolder.SetActive(false);
        containerTransform.localScale = previousScale; // Reset the scale to the minimum value
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "AircraftSelection" });
        aircraftSelection = savedData["AircraftSelection"].Value.GetAs<string>(); // Fetch the value held at this key
        InitialAircraftSelectionDisplay();
    }

    private void InitialAircraftSelectionDisplay()
    {
        switch (aircraftSelection)
        {
            case "Typhoon": // Check the given text against each possible aircraft
                typhoonOutline.SetActive(true); // If valid, set outline active on given aircraft
                currentOutline = typhoonOutline; // Set currentOutline to this outline
                break;
            case "Eagle": // Same as above
                eagleOutline.SetActive(true);
                currentOutline = eagleOutline;
                break;
            case "Business Jet": // Same as above
                businessJetOutline.SetActive(true);
                currentOutline = businessJetOutline;
                break;
            case "Rafale": // Same as above
                rafaleOutline.SetActive(true);
                currentOutline = rafaleOutline;
                break;
            default:
                break;
        }
    }
    public void Hide()
    {
        hideUI = true; // When the back button is pressed, set hideUI to true
    }

    private bool CheckInputText(string inputString)
    {
        inputString = inputString.ToLower(); // convert input to lower case
        switch (inputString)
        {
            case "typhoon": // Check against each aircraft, for the right aircraft:
                searchedAircraftText.text = "Typhoon"; // Set the text of the button to the name of the aircraft
                notYetImplementedObject.SetActive(false);
                return true; // Return true
            case "eagle":
                searchedAircraftText.text = "Eagle";
                notYetImplementedObject.SetActive(false);
                return true;
            case "business jet":
                searchedAircraftText.text = "Business Jet";
                notYetImplementedObject.SetActive(true);
                return true;
            case "rafale":
                searchedAircraftText.text = "Rafale";
                notYetImplementedObject.SetActive(true);
                return true;
            default:
                return false; // Else return false
        }
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

    public void ShowConfirmationPage()
    {
        Show();
        aircraftSelectionHeaderText.text = "confirm aircraft";
        confirmButtonHolder.SetActive(true);
    }
}
