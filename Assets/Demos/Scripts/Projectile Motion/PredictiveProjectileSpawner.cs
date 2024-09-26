using Obvious.Soap;
using UnityEngine;
using UnityEngine.AI;

public class PredictiveProjectileSpawner : ProjectileSpawner
{
    [Header("Predictive Target Settings")]
    [Tooltip("The target the projectile is aimed at.")]
    [SerializeField] private Transform target;

    [Tooltip("The Rigidbody of the target to predict future movement.")]
    [SerializeField] private NavMeshAgent targetNavMeshAgent;

    [Tooltip("Time to impact for predicting the future position of the target.")]
    [SerializeField] private FloatVariable timeToImpact;

    private Vector3 futureTargetPosition;

    ///// <summary>
    ///// Calculates the launch direction and initial velocity based on the predicted future position of the target and the shooting angle.
    ///// </summary>
    protected override void CalculateLaunchParameters()
    {
        // Predict the future position of the target using its velocity and time to impact
        futureTargetPosition = PredictFuturePosition();

        // Calculate the displacement to the future target position
        Vector3 displacement = futureTargetPosition - spawnPoint.position;

        // Separate horizontal and vertical components of the displacement
        float x = new Vector2(displacement.x, displacement.z).magnitude; // Only x and z contribute to horizontal distance
        float y = displacement.y;  // y is the vertical displacement

        // Time to impact
        float t = timeToImpact.Value;

        // Gravity constant (downward acceleration due to gravity)
        float g = Mathf.Abs(Physics.gravity.y);  // Use absolute value since gravity is negative

        // Horizontal velocity component (v0x)
        float v0x = x / t;

        // Vertical velocity component (v0y), derived from kinematic equation
        // y = v0y * t - 0.5 * g * t^2 => v0y = (y + 0.5 * g * t^2) / t
        float v0y = (y + 0.5f * g * t * t) / t;

        // Calculate horizontal launch direction (normalized to keep direction intact)
        Vector3 horizontalLaunchDirection = new Vector3(displacement.x, 0, displacement.z).normalized;

        // Final velocity vector is a combination of horizontal and vertical components
        Vector3 velocity = horizontalLaunchDirection * v0x + Vector3.up * v0y;

        // Set the initial velocity based on the calculated velocity components
        SetInitialVelocity(velocity);

        // Calculate maximum height (h_max) based on v0y and gravity
        float h_max = (v0y * v0y) / (2 * g);

        // Time to reach maximum height
        float t_max = v0y / g;

        // Calculate the horizontal displacement at the time of maximum height
        float horizontalDistanceAtMaxHeight = v0x * t_max;

        // Calculate the 3D position of the projectile at maximum height
        Vector3 maxHeightPosition = spawnPoint.position + horizontalLaunchDirection * horizontalDistanceAtMaxHeight;
        maxHeightPosition.y = spawnPoint.position.y + h_max;

        curveMaxHeightTransform.position = maxHeightPosition;
    }

    /// <summary>
    /// Predicts the future position of the target based on its current velocity and time to impact.
    /// </summary>
    private Vector3 PredictFuturePosition()
    {
        // Predicted future position = current position + velocity * time to impact
        return target.position + targetNavMeshAgent.velocity * timeToImpact.Value;
    }
}
