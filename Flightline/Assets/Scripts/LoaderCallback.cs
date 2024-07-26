using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;

    private void Update()
    {
        if (isFirstUpdate) // If this is the first update
        {
            isFirstUpdate = false; // Set isFirstUpdate to false

            Loader.LoaderCallback(); // Load the target scene
        }
    }
}