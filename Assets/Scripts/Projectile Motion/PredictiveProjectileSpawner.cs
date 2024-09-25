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

    /// <summary>
    /// Calculates the launch direction and initial velocity based on the predicted future position of the target.
    /// </summary>
    protected override void CalculateLaunchParameters()
    {
        // Predict the future position of the target using its velocity and time to impact
        futureTargetPosition = PredictFuturePosition();

        // Calculate the displacement to the future target position
        Vector3 displacement = futureTargetPosition - spawnPoint.position;

        // Separate horizontal and vertical components
        float horizontalDistance = new Vector2(displacement.x, displacement.z).magnitude;
        float verticalDistance = displacement.y;

        // Calculate the time needed to impact based on the horizontal distance and predefined timeToImpact
        float t = timeToImpact.Value;

        // Gravity constant (could be adjusted based on the game’s environment settings)
        float g = Physics.gravity.y;

        // Calculate the required total velocity magnitude using the horizontal distance and time to impact
        // This is derived from the horizontal motion equation: horizontalDistance = V_horizontal * t
        float horizontalVelocity = horizontalDistance / (t * Mathf.Cos(upwardAngle.Value * Mathf.Deg2Rad));

        // Calculate the vertical velocity component using kinematic equations for vertical motion
        // This accounts for the vertical distance and gravitational acceleration over time
        float verticalVelocity = (verticalDistance + 0.5f * Mathf.Abs(g) * t * t) / t;

        // Calculate the total initial velocity magnitude by combining the horizontal and vertical velocities
        float totalVelocityMagnitude = Mathf.Sqrt(horizontalVelocity * horizontalVelocity + verticalVelocity * verticalVelocity);

        // Set the launch force using the total velocity magnitude
        SetLaunchForce(totalVelocityMagnitude);

        // Calculate the launch direction for horizontal movement (ignoring Y-axis for now)
        Vector3 horizontalDirection = new Vector3(displacement.x, 0, displacement.z).normalized;

        // Combine the horizontal and vertical components into the launch direction and velocity
        Vector3 velocity = horizontalDirection * horizontalVelocity + Vector3.up * verticalVelocity;

        // Set the initial velocity based on the calculated velocity components
        SetInitialVelocity(velocity);
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
