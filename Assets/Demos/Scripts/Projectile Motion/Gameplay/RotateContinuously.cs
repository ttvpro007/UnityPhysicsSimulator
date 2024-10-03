using Sirenix.OdinInspector;
using UnityEngine;

public class RotateContinuously : MonoBehaviour
{
    // Enum to represent possible combinations of axes and planes for rotation
    public enum RotationAxis
    {
        X,
        Y,
        Z,
        XY,
        XZ,
        YZ,
        XYZ
    }

    [SerializeField] private bool randomRotateAtStart = false;
    [HideIf("@randomRotateAtStart")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private float speed = 10f;

    private void Start()
    {
        if (randomRotateAtStart)
        {
            ChooseRandomRotationAxis();
            SetRandomRotation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        RotateBasedOnSelection();
    }

    private void RotateBasedOnSelection()
    {
        Vector3 rotationVector = Vector3.zero;

        // Determine the rotation axis or plane based on the selected enum value
        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotationVector = Vector3.right;
                break;
            case RotationAxis.Y:
                rotationVector = Vector3.up;
                break;
            case RotationAxis.Z:
                rotationVector = Vector3.forward;
                break;
            case RotationAxis.XY:
                rotationVector = Vector3.right + Vector3.up;
                break;
            case RotationAxis.XZ:
                rotationVector = Vector3.right + Vector3.forward;
                break;
            case RotationAxis.YZ:
                rotationVector = Vector3.up + Vector3.forward;
                break;
            case RotationAxis.XYZ:
                rotationVector = Vector3.right + Vector3.up + Vector3.forward;
                break;
        }

        // Rotate based on the determined rotation vector, scaled by speed and deltaTime for smooth rotation
        transform.Rotate(rotationVector.normalized, speed * Time.deltaTime);
    }

    private void ChooseRandomRotationAxis()
    {
        // Get all values from the RotationAxis enum and randomly pick one
        RotationAxis[] rotationAxes = (RotationAxis[])System.Enum.GetValues(typeof(RotationAxis));
        rotationAxis = rotationAxes[Random.Range(0, rotationAxes.Length)];
    }

    private void SetRandomRotation()
    {
        // Set a random rotation for the GameObject
        transform.Rotate(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
    }
}
