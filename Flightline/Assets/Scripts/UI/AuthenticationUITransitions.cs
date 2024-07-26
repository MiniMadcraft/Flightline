using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationUITransitions : MonoBehaviour
{
    public static AuthenticationUITransitions Instance { get; private set; }
    public CanvasGroup mainInterfaceCanvasGroup;
    public CanvasGroup loadingText;
    [SerializeField] Button chooseSignInButton;
    [SerializeField] Button chooseSignUpButton;
    [SerializeField] Button chooseGuestSignInButton;
    [SerializeField] Button chooseAccountInformationButton;
    [SerializeField] Button signInPageBackButton;
    [SerializeField] Button signUpPageBackButton;
    [SerializeField] Button accountInformationPageBackButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button okButton;
    [SerializeField] GameObject choosePage;
    [SerializeField] GameObject signInPage;
    [SerializeField] GameObject signUpPage;
    [SerializeField] GameObject accountInformationPage;
    [SerializeField] GameObject errorPage;
    [SerializeField] GameObject okButtonGameObject;
    [SerializeField] CanvasGroup choosePageCanvasGroup;
    [SerializeField] CanvasGroup signInPageCanvasGroup;
    [SerializeField] CanvasGroup signUpPageCanvasGroup;
    [SerializeField] CanvasGroup accountInformationPageCanvasGroup;
    [SerializeField] CanvasGroup errorPageCanvasGroup;
    [SerializeField] TextMeshProUGUI errorPageHeaderText;
    [SerializeField] public TextMeshProUGUI errorPageMainText;
    private GameObject previousPage;
    private CanvasGroup previousPageCanvasGroup;
    private GameObject nextPage;
    private CanvasGroup nextPageCanvasGroup;
    private bool onLoad = true;
    private bool fadeIn = false;
    private bool fadeOut = false;
    private float time;
    private bool transition = false;
    private bool showErrorPage = false;
    private bool hideErrorPage = false;

    private void Start()
    {
        Instance = this;
        previousPage = choosePage; // Set the previousPage to the choosePage as this is the default page
        previousPageCanvasGroup = choosePageCanvasGroup; // Set the previousPageCanvasGroup to the choosePage's canvas group as this is the default page
        mainInterfaceCanvasGroup.alpha = 0f; // Set the alpha to 0 initially
        loadingText.alpha = 0f; // Set the loading text alpha to 0 initially
        chooseSignInButton.onClick.AddListener(() => // On clicking the chooseSignIn button..
        {
            if (!transition && !fadeIn && time > 2f) // Check if it is not currently transition IN to the current page otherwise the transitions will overlap and break
            {
                transition = true; // Set transition to true
                nextPage = signInPage; // Set the nextPage to the signInPage
                nextPageCanvasGroup = signInPageCanvasGroup; // Set the nextPageCanvasGroup to the signInPageCanvasGroup
                nextPage.gameObject.SetActive(true); // Set the game object active
            }
        });
        chooseSignUpButton.onClick.AddListener(() => // On clicking the chooseSignUp button...
        {
            if (!transition && !fadeIn && time > 2f) // As above
            {
                transition = true; // Set transition to true
                nextPage = signUpPage; // Set the nextPage to the signUpPage
                nextPageCanvasGroup = signUpPageCanvasGroup; // Set the nextPageCanvasGroup to the signUpPageCanvasGroup
                nextPage.gameObject.SetActive(true); // Set the game object active
            }
        });
        chooseGuestSignInButton.onClick.AddListener(async () => // On clicking the chooseGuestSignInButton...
        {
            if (!transition && !fadeIn && time > 2f) // As above
            {
                ErrorPage();
                okButtonGameObject.SetActive(false);
                errorPageHeaderText.text = "loading";
                errorPageMainText.text = "\n\n\n\nsigning in as a guest...";
                await GuestLoginHandler.Instance.SignInAnonymously(); // Sign in anonymously
            }
        });
        chooseAccountInformationButton.onClick.AddListener(() => // On clicking the chooseAccountInformation button..
        {
            if (!transition && !fadeIn && time > 2f) // As above
            {
                transition = true; // Set transition to true
                nextPage = accountInformationPage; // Set the nextPage to the accountInformationPage
                nextPageCanvasGroup = accountInformationPageCanvasGroup; // Set the nextPageCanvasGroup to the accountInformationPageCanvasGroup
                nextPage.gameObject.SetActive(true); // Set the game object active
            }
        });
        signInPageBackButton.onClick.AddListener(() => // When clicking the back button
        {
            if (transition != true) // As above
            {
                transition = true; // Set transition to true
                nextPage = choosePage; // Set the nextPage to the choosePage
                nextPageCanvasGroup = choosePageCanvasGroup; // Set the nextPage to the choosePageCanvasGroup
                nextPage.gameObject.SetActive(true); // Set the game object active
            }
        });
        signUpPageBackButton.onClick.AddListener(() => // As above
        {
            if (transition != true)
            {
                transition = true;
                nextPage = choosePage;
                nextPageCanvasGroup = choosePageCanvasGroup;
                nextPage.gameObject.SetActive(true);
            }
        });
        accountInformationPageBackButton.onClick.AddListener(() => // As above
        {
            if (transition != true)
            {
                transition = true;
                nextPage = choosePage;
                nextPageCanvasGroup = choosePageCanvasGroup;
                nextPage.gameObject.SetActive(true);
            }
        });
        quitButton.onClick.AddListener(() => // On clicking the quit button
        {
            Application.Quit(); // Quit the application
        });
        okButton.onClick.AddListener(() => // On clicking the ok button
        {
            hideErrorPage = true; // Set hideErrorPage to true as the user has acknowledged the error in sign in/sign up
        });
    }
    private void Update()
    {
        time += Time.deltaTime; // Creating an artificial delay
        if (onLoad && time > 2f) // If the scene has just loaded and the time since load is > 2 seconds
        {
            onLoad = false; // Set onLoad to false
            fadeIn = true; // Set fadeIn to true
            FadeInTransition(); // Play the fade in transition
        }
        else if (fadeIn) // If fadeIn is true
        {
            FadeInTransition(); // Play the transition
        }
        else if (fadeOut) // If fadeOut is true
        {
            FadeOutTransition(); // Play the fadeOut transition
        }
        else if (transition) // If transitioning between 2 pages
        {
            PlayPageTransitions(); // Play the page transitions
        }
        else if (showErrorPage && errorPage.gameObject.activeSelf) // If an error in signing in/signing up + ensuring game object is active before incrementing alpha
        {
            FadeInErrorPage(); // Play the transition
        }
        else if (hideErrorPage) // If the user has acknowledged the error page
        {
            FadeOutErrorPage(); // Play the transition
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) // If the escape key is pressed to bring up the pause menu, and the pause menu is not currently active
        {
            if (transition != true)
            {
                transition = true;
                nextPage = choosePage;
                nextPageCanvasGroup = choosePageCanvasGroup;
                nextPage.gameObject.SetActive(true);
            }
        }
    }

    private void FadeInTransition()
    {
        if (mainInterfaceCanvasGroup.alpha != 1f) // If alpha not yet 1
        {
            mainInterfaceCanvasGroup.alpha += Time.deltaTime; // Increment
        }
        else
        {
            fadeIn = false; // Set fadeIn to false as finished fading in
        }
    }

    public void PlayFadeOut()
    {
        fadeOut = true; // Set fadeOut to true as the user has signed in successfully
    }

    private void FadeOutTransition()
    {
        if (mainInterfaceCanvasGroup.alpha != 0f) // If not yet 0 alpha
        {
            mainInterfaceCanvasGroup.alpha -= Time.deltaTime * 3; // Decrement
        }
        if (loadingText.alpha != 1f) // If not yet 1 alpha
        {
            loadingText.alpha += Time.deltaTime * 2; // Increment at a slower rate
        }
        if (mainInterfaceCanvasGroup.alpha == 0f && loadingText.alpha == 1f) // If both at required values
        {
            fadeOut = false; // Fade out finished
            Loader.Load(Loader.Scene.MainMenuScene); // Load the main menu scene
        }
    }

    private void PlayPageTransitions()
    {
        if (previousPageCanvasGroup.alpha != 0f) // If the previous page is not yet 0 alpha
        {
            previousPageCanvasGroup.alpha -= Time.deltaTime * 3; // Decrement
        }
        if (nextPageCanvasGroup.alpha != 1f) // If the next page is not yet 1 alpha
        {
            nextPageCanvasGroup.alpha += Time.deltaTime * 3; // Increment
        }
        if (previousPageCanvasGroup.alpha == 0f && nextPageCanvasGroup.alpha == 1f) // If both at required alpha's
        {
            previousPage.gameObject.SetActive(false); // Set previous page inactive
            previousPage = nextPage; // Set the previousPage to the page just transitioned to
            previousPageCanvasGroup = nextPageCanvasGroup; // Same for canvas group
            transition = false; // Set transition to false as finished
        }
    }

    public void ErrorPage()
    {
        errorPage.gameObject.SetActive(true); // Set the error page active
        showErrorPage = true; // Set the bool to true to play the transition
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
}
