using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]
    /// <summary>
    /// The projectile prefab to spawn.
    /// </summary>
    [Tooltip("The projectile prefab to spawn.")]
    [SerializeField] private GameObject projectilePrefab;

    /// <summary>
    /// The point where the projectile will spawn.
    /// </summary>
    [Tooltip("The point where the projectile will spawn.")]
    [SerializeField] private Transform spawnPoint;

    /// <summary>
    /// Minimum force to apply to the projectile.
    /// </summary>
    [Tooltip("Minimum force to apply to the projectile.")]
    [SerializeField] private float launchForceMin = 10f;

    /// <summary>
    /// Maximum force to apply to the projectile.
    /// </summary>
    [Tooltip("Maximum force to apply to the projectile.")]
    [SerializeField] private float launchForceMax = 100f;

    /// <summary>
    /// Adjustable angle in degrees for upward launch.
    /// </summary>
    [Tooltip("Adjustable angle in degrees for upward launch.")]
    [SerializeField] private float upwardAngle = 45f;

    /// <summary>
    /// Time step for trajectory calculation.
    /// </summary>
    [Tooltip("Time step for trajectory calculation.")]
    [SerializeField] private float trajectoryTimeStep = 5f;

    [Header("References")]
    /// <summary>
    /// Reference to the TrajectoryDrawer component.
    /// </summary>
    [Tooltip("Reference to the TrajectoryDrawer component.")]
    [SerializeField] private TrajectoryDrawer trajectoryDrawer;

    private Vector3 launchDirection;
    private Vector3 initialVelocity;
    private float launchForce;

    /// <summary>
    /// Sets the launch force, clamping it within the min and max limits.
    /// </summary>
    /// <param name="launchForce">The desired launch force.</param>
    public void SetLaunchForce(float launchForce)
    {
        this.launchForce = Mathf.Clamp(launchForce, launchForceMin, launchForceMax);
    }

    private void Start()
    {
        // Initialize the launch force to the minimum value at the start
        launchForce = launchForceMin;
    }

    private void Update()
    {
        // Update the launch parameters each frame to account for changes in force or angle
        CalculateLaunchParameters();

        // Draw the trajectory in real-time
        DrawTrajectory();

        // Spawn the projectile when the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            SpawnProjectile();
        }
    }

    /// <summary>
    /// Calculates the launch direction and initial velocity based on the current parameters.
    /// </summary>
    private void CalculateLaunchParameters()
    {
        // Calculate launch direction with the upward angle
        launchDirection = Quaternion.AngleAxis(-upwardAngle, spawnPoint.right) * spawnPoint.forward;

        // Calculate initial velocity based on the launch force and direction
        initialVelocity = launchDirection.normalized * launchForce;
    }

    /// <summary>
    /// Draws the trajectory in real-time using the TrajectoryDrawer component.
    /// </summary>
    private void DrawTrajectory()
    {
        if (trajectoryDrawer == null || spawnPoint == null) return;

        // Call the DrawTrajectory method on the TrajectoryDrawer component
        trajectoryDrawer.DrawTrajectory(spawnPoint.position, initialVelocity, trajectoryTimeStep, Physics.gravity);
    }

    /// <summary>
    /// Spawns the projectile and applies the calculated launch force to it.
    /// </summary>
    private void SpawnProjectile()
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        // Instantiate the projectile at the spawn point's position and rotation
        GameObject spawnedProjectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        // Get the Rigidbody component of the spawned projectile
        Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Apply force in the calculated direction
            rb.AddForce(initialVelocity, ForceMode.Impulse);
        }
    }
}
