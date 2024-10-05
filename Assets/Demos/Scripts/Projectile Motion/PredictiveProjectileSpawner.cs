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

    [Tooltip("Fixed launch speed of the projectile.")]
    [SerializeField] private FloatVariable fixedLaunchSpeed;

    [SerializeField] private bool useFixedLaunchSpeed;
    [SerializeField] private float yOffset;

    private Vector3 futureTargetPosition;

    protected override void CalculateLaunchParameters()
    {
        if (useFixedLaunchSpeed)
        {
            CalculateLaunchParametersByLaunchSpeed();
        }
        else
        {
            CalculateLaunchParametersByTimeToImpact();
        }
    }

    ///// <summary>
    ///// Calculates the launch direction and initial velocity based on the predicted future position of the target and the shooting angle.
    ///// </summary>
    private void CalculateLaunchParametersByTimeToImpact()
    {
        // Predict the future position of the target using its velocity and time to impact
        futureTargetPosition = PredictFuturePosition();
        futureTargetPosition.y += yOffset;

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

        // vy = tan (theta) * vx
        //float theta = upwardAngle * Mathf.Deg2Rad;
        //float v0y = Mathf.Tan(theta) * v0x;

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
    /// Calculates the launch direction and initial velocity to hit a moving target
    /// using a fixed launch speed, including a range check based on the optimal launch angle.
    /// </summary>
    private void CalculateLaunchParametersByLaunchSpeed()
    {
        // Predict the future position of the target using its velocity and time to impact
        futureTargetPosition = PredictFuturePosition();

        // Calculate the displacement to the future target position
        Vector3 displacement = futureTargetPosition - spawnPoint.position;

        // Separate horizontal and vertical components of the displacement
        float x = new Vector2(displacement.x, displacement.z).magnitude; // Horizontal distance (x-z plane)
        float y = displacement.y;  // Vertical displacement

        // Gravity constant (downward acceleration due to gravity)
        float g = Mathf.Abs(Physics.gravity.y);  // Use absolute value since gravity is negative

        // Fixed launch speed
        float v0 = fixedLaunchSpeed; // Set your fixed launch speed here

        // Step 1: Calculate Maximum Range at Optimal Launch Angle (45 degrees)
        // R_max = (v0^2 * sin(2 * 45)) / g
        float maxRange = (v0 * v0 * Mathf.Sin(2 * 45 * Mathf.Deg2Rad)) / g;

        // Step 2: Check if the target is within the range
        if (x > maxRange)
        {
            Debug.LogWarning("Target is out of range. Maximum range is " + maxRange + " meters.");
            return;
        }

        // Step 3: Solve for the launch angle (theta)
        bool foundSolution = false;
        float optimalTheta = 0f;
        float t = 0f;

        for (float theta = 0; theta <= 90; theta += 0.1f)
        {
            float thetaRad = theta * Mathf.Deg2Rad;

            // Calculate horizontal and vertical components of velocity
            float v0x = v0 * Mathf.Cos(thetaRad);
            float v0y = v0 * Mathf.Sin(thetaRad);

            // Calculate time to impact using horizontal displacement and velocity
            if (Mathf.Abs(v0x) > 0.01f) // Avoid division by zero
            {
                t = x / v0x;

                // Check if the vertical position matches the target's vertical displacement at this time
                float calculatedY = (v0y * t) - (0.5f * g * t * t);

                // Allow for a small tolerance due to numerical precision issues
                if (Mathf.Abs(calculatedY - y) < 0.5f)
                {
                    optimalTheta = thetaRad;
                    foundSolution = true;
                    break;
                }
            }
        }

        // If no solution is found, handle accordingly (e.g., set default values or log an error)
        if (!foundSolution)
        {
            Debug.LogWarning("No feasible launch angle found to hit the target.");
            return;
        }

        // Step 4: Calculate the launch parameters based on the found angle
        // Calculate horizontal launch direction (normalized to keep direction intact)
        Vector3 horizontalLaunchDirection = new Vector3(displacement.x, 0, displacement.z).normalized;

        // Calculate initial velocity components based on the found launch angle
        float finalV0x = v0 * Mathf.Cos(optimalTheta);
        float finalV0y = v0 * Mathf.Sin(optimalTheta);

        // Final velocity vector is a combination of horizontal and vertical components
        Vector3 velocity = horizontalLaunchDirection * finalV0x + Vector3.up * finalV0y;

        // Set the initial velocity based on the calculated velocity components
        SetInitialVelocity(velocity);

        // Step 5: Calculate maximum height (h_max) based on v0y and gravity
        float h_max = (finalV0y * finalV0y) / (2 * g);

        // Time to reach maximum height
        float t_max = finalV0y / g;

        // Calculate the horizontal displacement at the time of maximum height
        float horizontalDistanceAtMaxHeight = finalV0x * t_max;

        // Calculate the 3D position of the projectile at maximum height
        Vector3 maxHeightPosition = spawnPoint.position + horizontalLaunchDirection * horizontalDistanceAtMaxHeight;
        maxHeightPosition.y = spawnPoint.position.y + h_max;

        // Set the position of the max height indicator (for visualization purposes)
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
