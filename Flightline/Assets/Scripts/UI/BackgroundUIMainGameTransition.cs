using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundUIMainGameTransition : MonoBehaviour
{
    public static BackgroundUIMainGameTransition Instance { get; private set; }

    public CanvasGroup mainCanvasGroup;
    public CanvasGroup loadingTextCanvasGroup;
    private bool transitionIn = true;
    private bool transitionOut = false;
    private float time = 0f; // Add an artifical delay to ensure that everything has been loaded and the aircraft is not floating

    private void Start()
    {
        Instance = this;
    }
    private void Update()
    {
        time += Time.unscaledDeltaTime;
        if (transitionIn && time > 3f) // If transition not played...
        {
            PlayTransitionIn(); // Play the transition
        }
        if (transitionOut) // If transitioning out
        {
            PlayTransitionOut(); // Play the transition
        }
    }

    private void PlayTransitionIn()
    {
        if (mainCanvasGroup.alpha != 0f) // If the alpha value is not 0
        {
            mainCanvasGroup.alpha -= Time.unscaledDeltaTime * 4; // Decrement with respect to UNSCALED DELTA TIME
            loadingTextCanvasGroup.alpha -= Time.unscaledDeltaTime * 4; // Decrement with respect to UNSCALED DELTA TIME
        }
        else // If it is 0
        {
            transitionIn = false; // The transition has been played
            gameObject.SetActive(false); // Disable the game object
        }
    }

    private void PlayTransitionOut()
    {
        if (mainCanvasGroup.alpha != 1f) // If the alpha value is not yet 1
        {
            mainCanvasGroup.alpha += Time.unscaledDeltaTime * 4; // Increment with respect to UNSCALED DELTA TIME
            loadingTextCanvasGroup.alpha += Time.unscaledDeltaTime * 4; // Increment with respect to UNSCALED DELTA TIME
        }
        else
        {
            transitionOut = false; // The transition has been played
            Time.timeScale = 1f; // Reset time scale
            Loader.Load(Loader.Scene.MainMenuScene); // Load the main menu scene
        }
    }

    public void TransitionOut()
    {
        transitionOut = true; // Set bool true
        gameObject.SetActive(true); // Enable the game object
    }
}
