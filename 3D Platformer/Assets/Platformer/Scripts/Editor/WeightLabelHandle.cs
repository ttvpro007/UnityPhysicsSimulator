using UnityEngine;
using UnityEditor;
using AI.PatrolPath;

[CustomEditor(typeof(WeightHandle))]
public class WeightLabelHandle : Editor
{
    WeightHandle weightHandle;

    private void OnSceneGUI()
    {
        weightHandle = (WeightHandle)target;

        if (weightHandle == null || weightHandle.weights.Count == 0) return;

        Handles.color = Color.red;

        for (int i = 0; i < weightHandle.textPositions.Count; i++)
        {
            Handles.Label(weightHandle.textPositions[i], weightHandle.weights[i].ToString("0.00") + " meter(s)");
        }
    }
}