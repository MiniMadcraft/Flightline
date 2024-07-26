using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // On interaction with the Inspector...

        UpdatableData data = (UpdatableData)target;

        if (GUILayout.Button("Update")) // If the interaction was with the "Update" button..
        {
            data.NotifyOfUpdatedValues(); // Notify to update values used
            EditorUtility.SetDirty(target);
        }
    }
}
