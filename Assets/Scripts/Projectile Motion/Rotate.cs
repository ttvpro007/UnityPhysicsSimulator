using UnityEngine;

public class Rotate: MonoBehaviour
{
    [SerializeField] Transform runner;  // Reference to the runner
    [SerializeField] float rotationSpeed = 5f;  // Speed at which the thrower rotates to face the runner
    [SerializeField] Transform directionIndicator;  // Object under the thrower hierarchy that will show the direction

    private void Update()
    {
        if (runner == null) return;

        // Get the direction to the runner on the XZ plane (ignore Y-axis)
        Vector3 directionToRunner = runner.position - transform.position;
        directionToRunner.y = 0f;  // Keep the direction strictly in the XZ plane

        // Rotate the thrower to face the runner
        RotateThrower(directionToRunner);

        // Rotate the direction indicator to show the direction to the runner
        RotateDirectionIndicator(directionToRunner);
    }

    // Function to rotate the thrower to face the runner
    private void RotateThrower(Vector3 directionToRunner)
    {
        if (directionToRunner != Vector3.zero)
        {
            // Calculate the target rotation to look at the runner
            Quaternion targetRotation = Quaternion.LookRotation(directionToRunner);

            // Smoothly rotate the thrower towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Function to rotate the direction indicator to point at the runner
    private void RotateDirectionIndicator(Vector3 directionToRunner)
    {
        if (directionIndicator != null)
        {
            // Calculate the target rotation for the direction indicator
            Quaternion targetRotation = Quaternion.LookRotation(directionToRunner);

            // Directly set the rotation of the direction indicator
            directionIndicator.rotation = targetRotation;
        }
    }
}
