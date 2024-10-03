using Obvious.Soap;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]
    /// <summary>
    /// The projectile prefab to spawn.
    /// </summary>
    [Tooltip("The projectile prefab to spawn.")]
    [SerializeField] protected GameObject projectilePrefab;

    /// <summary>
    /// The point where the projectile will spawn.
    /// </summary>
    [Tooltip("The point where the projectile will spawn.")]
    [SerializeField] protected Transform spawnPoint;

    /// <summary>
    /// Adjustable launch force.
    /// </summary>
    [Tooltip("Adjustable launch force.")]
    [SerializeField] protected FloatVariable launchForce;

    /// <summary>
    /// Adjustable angle in degrees for upward launch.
    /// </summary>
    [Tooltip("Adjustable angle in degrees for upward launch.")]
    [SerializeField] protected FloatVariable upwardAngle;

    /// <summary>
    /// Time step for trajectory calculation.
    /// </summary>
    [Tooltip("Time step for trajectory calculation.")]
    [SerializeField] protected float trajectoryTimeStep = 5f;

    [Header("References")]
    /// <summary>
    /// Reference to the TrajectoryDrawer component.
    /// </summary>
    [Tooltip("Reference to the TrajectoryDrawer component.")]
    [SerializeField] protected TrajectoryDrawer trajectoryDrawer;

    /// <summary>
    /// Speed at which the thrower rotates.
    /// </summary>
    [Tooltip("Speed at which the thrower rotates.")]
    [SerializeField] protected float rotationSpeed = 5f;  // Speed at which the thrower rotates to face the runner

    [SerializeField] protected Transform curveMaxHeightTransform;

    protected Vector3 launchDirection;
    private Vector3 initialVelocity;

    /// <summary>
    /// Sets the launch force.
    /// </summary>
    /// <param name="launchForce">The desired launch force.</param>
    public void SetLaunchForce(float launchForce)
    {
        this.launchForce.Value = launchForce;
    }

    private void Update()
    {
        // Update the launch parameters each frame to account for changes in force or angle
        CalculateLaunchParameters();

        // Draw the trajectory in real-time
        DrawTrajectory();

        RotateThrower();
    }

    public void SetInitialVelocity(Vector3 initialVelocity)
    {
        this.initialVelocity = initialVelocity;

        spawnPoint.LookAt(initialVelocity);
    }

    public void SetProjectile(GameObject projectilePrefab)
    {
        this.projectilePrefab = projectilePrefab;
    }

    /// <summary>
    /// Calculates the launch direction and initial velocity based on the current parameters.
    /// </summary>
    protected virtual void CalculateLaunchParameters()
    {
        // Calculate launch direction with the upward angle
        launchDirection = Quaternion.AngleAxis(-upwardAngle.Value, spawnPoint.right) * spawnPoint.forward;

        // Calculate initial velocity based on the launch force and direction
        SetInitialVelocity(launchDirection.normalized * launchForce.Value);
    }

    /// <summary>
    /// Draws the trajectory in real-time using the TrajectoryDrawer component.
    /// </summary>
    protected virtual void DrawTrajectory()
    {
        if (trajectoryDrawer == null || spawnPoint == null) return;

        // Call the DrawTrajectory method on the TrajectoryDrawer component
        trajectoryDrawer.DrawTrajectory(spawnPoint.position, initialVelocity, trajectoryTimeStep, Physics.gravity);
    }

    // Function to rotate the thrower to face the runner
    private void RotateThrower()
    {
        Vector3 directionToTarget = initialVelocity.normalized;
        directionToTarget.y = 0f;  // Keep the direction strictly in the XZ plane

        // Calculate the target rotation to look at the runner
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Smoothly rotate the thrower towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Spawns the projectile and applies the calculated launch force to it.
    /// </summary>
    public void SpawnProjectile()
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        // Instantiate the projectile at the spawn point's position and rotation
        GameObject spawnedProjectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        Projectile projectile = spawnedProjectile.GetComponent<Projectile>();
        projectile.SetHitPointPosition(trajectoryDrawer.HitPointPosition);

        // Get the Rigidbody component of the spawned projectile
        if (spawnedProjectile.TryGetComponent<Rigidbody>(out var rb))
        {
            // Apply force in the calculated direction
            rb.AddForce(initialVelocity, ForceMode.Impulse);
        }
    }
}
