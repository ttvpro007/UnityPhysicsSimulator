using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject projectilePrefab;   // The projectile prefab to spawn
    public Transform spawnPoint;          // The point where the projectile will spawn

    public float launchForce = 10f;       // Force to apply to the projectile
    public float upwardAngle = 45f;       // Adjustable angle in degrees for upward launch
    public float trajectoryTimeStep = 5f; // Time step for trajectory calculation

    public TrajectoryDrawer trajectoryDrawer;  // Reference to the TrajectoryDrawer

    private void Update()
    {
        // Draw the trajectory in real-time
        DrawTrajectory();

        // Spawn the projectile when the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            SpawnProjectile();
        }
    }

    void DrawTrajectory()
    {
        // Check if we have a trajectory drawer assigned
        if (trajectoryDrawer == null || spawnPoint == null)
            return;

        // Calculate launch direction with upward angle
        Vector3 launchDirection = Quaternion.AngleAxis(-upwardAngle, spawnPoint.right) * spawnPoint.forward;

        // Calculate initial velocity based on launch force and direction
        Vector3 initialVelocity = launchDirection.normalized * launchForce;

        // Call the DrawTrajectory method on the TrajectoryDrawer component
        trajectoryDrawer.DrawTrajectory(spawnPoint.position, initialVelocity, trajectoryTimeStep, Physics.gravity);
    }

    void SpawnProjectile()
    {
        // Instantiate the projectile at the spawn point's position and rotation
        GameObject spawnedProjectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        // Get the Rigidbody component of the spawned projectile
        Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Calculate the launch direction with the upward angle
            Vector3 launchDirection = Quaternion.AngleAxis(-upwardAngle, spawnPoint.right) * spawnPoint.forward;

            // Apply force in the calculated direction
            rb.AddForce(launchDirection.normalized * launchForce, ForceMode.Impulse);
        }
    }
}
