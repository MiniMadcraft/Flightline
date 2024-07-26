using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    //this part is NOT compiled when the project is built - it is only for use in the EDITOR. Denoted by the #if and #endif
    #if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (autoUpdate) // If autoUpdate set to true...
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues; // Subscribe to the event
        }
    }
    public void NotifyOfUpdatedValues() // When the procedure is called
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues; // Unsubscribe from the event
        if (OnValuesUpdated != null) // If not null
        {
            OnValuesUpdated(); // Update the values
        }
    }
    #endif
}
