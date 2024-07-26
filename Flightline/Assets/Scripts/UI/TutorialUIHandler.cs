using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUIHandler : MonoBehaviour
{
    [SerializeField] GameObject tutorialObject;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] CanvasGroup tutorialBoxCanvasGroup;
    [SerializeField] PhysicsCalculations plane;

    private float time = 0f;
    private bool welcomeScreen = true;
    private bool throttleScreen = false;
    private bool pitchScreen = false;
    private bool rollScreen = false;

    private bool firstUpdate = true;
    private bool transitionIn = false;
    private bool transitionOut = false;

    private bool endOfTutorial = false;

    void Update()
    {
        time += Time.unscaledDeltaTime;
        if (time > 4f && firstUpdate)
        {
            transitionIn = true;
            firstUpdate = false;
            GameInput.Instance.playerInputActions.Plane.rollLeft.Disable();
            GameInput.Instance.playerInputActions.Plane.rollRight.Disable();
            GameInput.Instance.playerInputActions.Plane.throttleDown.Disable();
            GameInput.Instance.playerInputActions.Plane.pitchUp.Disable();
            GameInput.Instance.playerInputActions.Plane.pitchDown.Disable();
        }
        if (transitionIn)
        {
            PlayTransitionIn();
        }
        if (transitionOut)
        {
            PlayTransitionOut();
        }
        if (time > 8f && welcomeScreen) // Current state: Welcome screen
        {
            welcomeScreen = false;
            throttleScreen = true;
            GameInput.Instance.playerInputActions.Plane.throttleUp.Enable(); // Enable throttle increase inputs
            tutorialText.text = "in this tutorial, you will learn the basic controls of the aircraft in flightline. to start, press and hold " + GameInput.Instance.GetBindingText(GameInput.Binding.throttleUp) + " to increase your throttle";
        }
        if (throttleScreen && plane.Throttle > 0.85f) // Current state: Throttle screen
        {
            GameInput.Instance.playerInputActions.Plane.throttleUp.Disable(); // Disable throttle increase inputs
            pitchScreen = true;
            throttleScreen = false;
            tutorialText.text = "hold " + GameInput.Instance.GetBindingText(GameInput.Binding.pitchUp) + " to pitch your aircraft up";
            GameInput.Instance.playerInputActions.Plane.pitchUp.Enable(); // Enable pitch up inputs
        }
        if (pitchScreen && plane.transform.rotation.x > 0.015f) // Current state: Pitch screen
        {
            GameInput.Instance.playerInputActions.Plane.pitchUp.Disable(); // Disable pitch up inputs
            tutorialText.text = "wait until you reach 650ft";
        }
        if (pitchScreen && plane.transform.position.y > 65f) // Current state: Pitch screen
        {
            GameInput.Instance.playerInputActions.Plane.pitchDown.Enable(); // Enable pitch down inputs
            tutorialText.text = "hold " + GameInput.Instance.GetBindingText(GameInput.Binding.pitchDown) + " to level out flight";
        }
        if (pitchScreen && plane.transform.rotation.x < 0.015f && plane.transform.position.y > 65) // Current state: Roll screen
        {
            GameInput.Instance.playerInputActions.Plane.pitchDown.Disable(); // Disable pitch down inputs
            GameInput.Instance.playerInputActions.Plane.rollLeft.Enable(); // Enable roll left inputs
            rollScreen = true;
            pitchScreen = false;
            tutorialText.text = "hold " + GameInput.Instance.GetBindingText(GameInput.Binding.rollLeft) + " to roll left";
        }
        if (rollScreen && plane.transform.rotation.z < 0.02f) // Current state: Roll screen
        {
            GameInput.Instance.playerInputActions.Plane.rollLeft.Disable(); // Disable roll left inputs
            GameInput.Instance.playerInputActions.Plane.rollRight.Enable(); // Enable roll right inputs
            tutorialText.text = "hold " + GameInput.Instance.GetBindingText(GameInput.Binding.rollRight) + " to roll right and stabilise flight";
        }
        if (rollScreen && plane.transform.rotation.z > 0.04f && !GameInput.Instance.playerInputActions.Plane.rollLeft.enabled && GameInput.Instance.playerInputActions.Plane.rollRight.enabled) // Current state: End of tutorial
        {
            tutorialText.text = "you have now mastered the basic controls of flightline. press the play button in the main menu and start flying!";
            GameInput.Instance.playerInputActions.Plane.rollRight.Disable(); // Disable roll right inputs
            time = 0f;
            endOfTutorial = true;
        }
        if (endOfTutorial && time > 8f && tutorialObject.activeSelf) // Current state: End of tutorial
        {
            transitionOut = true;
        }
        else if (time > 10f && !transitionOut && endOfTutorial) // Current state: End of tutorial
        {
            BackgroundUIMainGameTransition.Instance.TransitionOut(); // Transition to main menu
        }
    }

    private void PlayTransitionIn()
    {
        if (tutorialBoxCanvasGroup.alpha != 1f)
        {
            tutorialBoxCanvasGroup.alpha += Time.unscaledDeltaTime * 3f;
        }
        else
        {
            transitionIn = false;
        }
    }

    private void PlayTransitionOut()
    {
        if (tutorialBoxCanvasGroup.alpha != 0f)
        {
            tutorialBoxCanvasGroup.alpha -= Time.unscaledDeltaTime * 3f;
        }
        else
        {
            transitionOut = false;
            tutorialObject.SetActive(false);
        }
    }
}
