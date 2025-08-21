using UnityEngine;

public class Arrow : Projectile
{
    // Serialized Fields
    [Tooltip("Delay before destroying the arrow after impact.")]
    [SerializeField] private float destroyDelay = 3f;

    // Private Fields
    private float countdown;
    private bool hasHit = false;
    private int currentPathIndex = 0;  // Index of the current point in TrajectoryPath

    // Unity Lifecycle Methods
    protected override void Start()
    {
        base.Start();
        // Set the countdown timer for destruction after impact
        countdown = destroyDelay;
    }

    private void Update()
    {
        if (hasHit)
        {
            // Countdown before destruction after hitting a target
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                Destroy();
            }
        }
        else
        {
            // Rotate along the trajectory while the arrow is in flight
            RotateAlongTrajectory();
        }
    }

    // Private Methods
    /// <summary>
    /// Rotates the arrow to follow its trajectory, based on the trajectory path provided by TrajectoryDrawer.
    /// </summary>
    private void RotateAlongTrajectory()
    {
        if (trajectoryDrawer == null || trajectoryDrawer.TrajectoryPath == null || trajectoryDrawer.TrajectoryPath.Count < 2)
        {
            return; // Exit if there's no valid trajectory path
        }

        // Ensure we are not at the end of the path
        if (currentPathIndex >= trajectoryDrawer.TrajectoryPath.Count - 1)
        {
            return; // No further rotation needed
        }

        // Rotate the arrow to align with the direction of its velocity
        if (rBody.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rBody.linearVelocity);
        }

        // Get the next point in the path
        Vector3 nextPoint = trajectoryDrawer.TrajectoryPath[currentPathIndex + 1];

        // Move to the next segment of the path if close enough to the current target point
        if (Vector3.Distance(transform.position, nextPoint) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    // Protected Methods
    /// <summary>
    /// Handles collision events to disable physics and optionally attach to the hit object.
    /// </summary>
    /// <param name="collision">Collision information from Unity's physics system.</param>
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Ignore collisions with specific tagged objects
        if (!collision.gameObject.CompareTag("Ignore Collision"))
        {
            // Deactivate rigidbody physics on impact
            rBody.isKinematic = true;

            // Attach the arrow to the target if it has a specific tag
            if (collision.gameObject.CompareTag("Collision Target"))
            {
                transform.SetParent(collision.transform);
            }
        }

        // Set flag indicating the arrow has hit something
        hasHit = true;
    }
}
