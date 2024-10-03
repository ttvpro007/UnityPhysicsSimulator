using UnityEngine;

public class Arrow : Projectile
{
    [SerializeField] private float destroyDelay = 3f;       // Delay before the grenade explodes
    [SerializeField] private TrajectoryDrawer trajectoryDrawer;

    private float countdown;
    private bool hasHit = false;
    private int currentPathIndex = 0;                       // Index of the current point in TrajectoryPath

    protected override void Awake()
    {
        base.Awake();

        trajectoryDrawer = FindAnyObjectByType<TrajectoryDrawer>();
    }

    protected override void Start()
    {
        base.Start();

        // Set the countdown timer
        countdown = destroyDelay;
    }

    private void Update()
    {
        // Countdown before destroying
        if (hasHit)
        {
            countdown -= Time.deltaTime;

            // Destroy after time is up
            if (countdown <= 0f)
            {
                Destroy();
            }
        }
        else
        {
            // Rotate along trajectory drawer path
            RotateAlongTrajectory();
        }
    }

    private void RotateAlongTrajectory()
    {
        if (trajectoryDrawer.TrajectoryPath == null || trajectoryDrawer.TrajectoryPath.Count < 2)
            return;

        // Ensure we are not at the end of the path
        if (currentPathIndex >= trajectoryDrawer.TrajectoryPath.Count - 1)
            return;

        // Get the current point and the next point in the path
        Vector3 currentPoint = trajectoryDrawer.TrajectoryPath[currentPathIndex];
        Vector3 nextPoint = trajectoryDrawer.TrajectoryPath[currentPathIndex + 1];

        // Calculate the direction from current position to the next point
        Vector3 direction = (nextPoint - transform.position).normalized;

        // Set Rigidbody velocity to move in that direction
        //rBody.velocity = direction * rBody.velocity.magnitude;

        // Rotate the arrow to align with the direction of velocity
        if (rBody.velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rBody.velocity);
        }

        // Check if the arrow is close enough to the next point to move to the next segment
        if (Vector3.Distance(transform.position, nextPoint) < 0.1f)
        {
            currentPathIndex++;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (!collision.gameObject.CompareTag("Ignore Collision"))
        {
            // Deactivate rigidbody physics
            rBody.isKinematic = true;

            if (collision.gameObject.CompareTag("Collision Target"))
            {
                transform.SetParent(collision.transform);
            }
        }

        hasHit = true;
    }
}
