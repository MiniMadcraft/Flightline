using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAircraftAnimationHandler : MonoBehaviour
{
    private Animator backgroundAnimation;
    private const string PLAY_ANIMATION = "PlayAnimation";
    private float time;
    private System.Random randomValue = new System.Random();
    public float threshold;

    private void Start()
    {
        backgroundAnimation = GetComponent<Animator>(); // Get the Animator component from the game object
    }
    private void Update()
    {
        time += Time.deltaTime; // Add time since last frame to the cumulative time
        CheckToPlayAnimation(); // Check if time exceeds threshold
    }

    private void CheckToPlayAnimation()
    {
        if (time >= threshold) // If time since last try greater than threshold
        {
            int val = randomValue.Next(1,5); // Randomise a value between1 and 5
            if (val > 2) // If between 3 - 5 AND the current animation has finished
            {
                backgroundAnimation.SetTrigger(PLAY_ANIMATION); // Play the animation
            }
            time = 0f; // Reset the time, wait for threshold to be reached and try again
        }
    }
}
