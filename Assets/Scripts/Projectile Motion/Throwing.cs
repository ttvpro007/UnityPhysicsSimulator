using System;
using UnityEngine;

public class Throwing : MonoBehaviour
{
    [SerializeField] Transform runner;  // Reference to the runner
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject grenade;
    [SerializeField] float throwingAngle;
    [SerializeField] float gravity = 9.8f;
    const float SpeedModifier = 4.4f;

    float theta;
    float tanTheta;

    //public float Gravity
    //{
    //    get { return gravity; }
    //}

    //public float Theta
    //{
    //    get { return throwingAngle * Mathf.PI / 180f; }
    //}

    //private void Update()
    //{
    //    if (runner == null || grenade == null) return;

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 predictedPosition = PredictRunnerPosition();
    //        Throw(predictedPosition);  // Pass the predicted position to the Throw method
    //    }
    //}

    //private void Throw(Vector3 targetPosition)
    //{
    //    // Instantiate the grenade at the spawn point
    //    GameObject grenadeInstance = Instantiate(grenade, spawnPoint.position, Quaternion.identity);

    //    // Get the GrenadeProperty script from the grenade instance
    //    GrenadeProperty grenadeScript = grenadeInstance.GetComponent<GrenadeProperty>();

    //    // If the script is found, calculate the velocity and apply it using ApplyForce
    //    if (grenadeScript != null)
    //    {
    //        Vector3 velocity = CalculateInitialVelocity(targetPosition);  // Calculate the velocity
    //        grenadeScript.ApplyForce(velocity);  // Use ApplyForce to apply the velocity
    //    }
    //}

    //private Vector3 CalculateInitialVelocity(Vector3 targetPosition)
    //{
    //    // Calculate the distance to the predicted position
    //    float distanceToTarget = Vector3.Distance(spawnPoint.position, targetPosition);
    //    float initialVelocity = Mathf.Sqrt(distanceToTarget * gravity / Mathf.Sin(2 * Theta));
    //    initialVelocity /= SpeedModifier;

    //    // Break velocity into x and y components based on throwing angle
    //    float velocityX = initialVelocity * Mathf.Cos(Theta);
    //    float velocityY = initialVelocity * Mathf.Sin(Theta);

    //    // Calculate direction to the target position
    //    Vector3 direction = (targetPosition - spawnPoint.position).normalized;
    //    Vector3 velocity = new Vector3(direction.x * velocityX, velocityY, direction.z * velocityX);

    //    return velocity;
    //}

    //private Vector3 PredictRunnerPosition()
    //{
    //    // Get runner's current velocity and position
    //    Running runnerScript = runner.GetComponent<Running>();
    //    Vector3 runnerPosition = runner.position;
    //    Vector3 runnerVelocity = runnerScript.CurrentVelocity;

    //    // Calculate the time it would take for the grenade to reach the runner
    //    float distanceToRunner = Vector3.Distance(spawnPoint.position, runnerPosition);
    //    float grenadeSpeed = CalculateInitialVelocity(runnerPosition).magnitude;
    //    float timeToReach = distanceToRunner / grenadeSpeed;

    //    // Predict the runner's future position based on their velocity and time to reach
    //    Vector3 predictedPosition = runnerPosition + runnerVelocity * timeToReach;

    //    return predictedPosition;
    //}
}
