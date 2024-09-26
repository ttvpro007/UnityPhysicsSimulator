using Obvious.Soap;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Running : MonoBehaviour
{
    [Header("Running Settings")]
    /// <summary>
    /// Boolean that determines whether the object is currently running.
    /// </summary>
    [Tooltip("Boolean that determines whether the object is currently running.")]
    [SerializeField] private bool run;

    /// <summary>
    /// Speed of the object when running, controlled by the NavMeshAgent.
    /// </summary>
    [Tooltip("Speed of the object when running, controlled by the NavMeshAgent.")]
    [SerializeField] private FloatVariable speed;

    /// <summary>
    /// Radius around the map center where the object will randomly pick new destinations.
    /// </summary>
    [Tooltip("Radius around the map center where the object will randomly pick new destinations.")]
    [SerializeField] private float radius = 10f;

    /// <summary>
    /// Distance tolerance to the destination. If the object is within this distance, it will pick a new destination.
    /// </summary>
    [Tooltip("Distance tolerance to the destination. If the object is within this distance, it will pick a new destination.")]
    [SerializeField] private float destinationTolerance = 2f;

    [Header("Map Settings")]
    /// <summary>
    /// The central point around which the object will move. The object will stay within the radius of this point.
    /// </summary>
    [Tooltip("The central point around which the object will move. The object will stay within the radius of this point.")]
    [SerializeField] private Transform mapCenter;

    [Header("Indicator Settings")]
    /// <summary>
    /// Optional: An indicator that shows the direction the object is currently moving in.
    /// </summary>
    [Tooltip("Optional: An indicator that shows the direction the object is currently moving in.")]
    [SerializeField] private Transform directionIndicator;

    /// <summary>
    /// Optional: An indicator that shows the target destination of the object.
    /// </summary>
    [Tooltip("Optional: An indicator that shows the target destination of the object.")]
    [SerializeField] Transform targetIndicator;

    // NavMeshAgent component that handles the object's movement
    private NavMeshAgent agent;

    // The current destination point for the object
    private Vector3 currentDestination;

    /// <summary>
    /// Public property to expose the current velocity of the NavMeshAgent.
    /// </summary>
    public Vector3 CurrentVelocity { get; private set; }

    private void Awake()
    {
        // Cache the NavMeshAgent component for performance
        agent = GetComponent<NavMeshAgent>();
        if (!agent)
        {
            Debug.LogError("NavMeshAgent component missing from this GameObject.");
            enabled = false;
        }
    }

    private void Start()
    {
        if (!mapCenter)
        {
            Debug.LogError("Map Center is not referenced.");
            enabled = false;
            return;
        }

        // Set the initial destination
        SetDestination();
    }

    private void Update()
    {
        if (!run) return;

        // Set the NavMeshAgent's movement speed
        agent.speed = speed.Value;

        // Update the current velocity of the agent
        CurrentVelocity = agent.velocity;

        // Check if the agent has reached its destination
        if (HasReachedDestination())
        {
            // If destination is reached, pick a new one
            SetDestination();
        }

        // Optionally, update the direction indicator to show the current running direction
        UpdateDirectionIndicator();
    }

    /// <summary>
    /// Checks if the object has reached the current destination by comparing the squared distance to the destination tolerance.
    /// </summary>
    /// <returns>True if the object is within the tolerance range of the destination, false otherwise.</returns>
    private bool HasReachedDestination()
    {
        // Use squared distance to avoid unnecessary square root calculations
        return (agent.transform.position - currentDestination).sqrMagnitude <= destinationTolerance * destinationTolerance;
    }

    /// <summary>
    /// Sets a new random destination within the radius around the map center.
    /// Updates the target indicator to reflect the new destination.
    /// </summary>
    private void SetDestination()
    {
        currentDestination = GenerateRandomPosition();
        agent.SetDestination(currentDestination);
        SetTargetIndicatorPosition(currentDestination);
    }

    /// <summary>
    /// Generates a random position within the defined radius around the map center.
    /// </summary>
    /// <returns>A new random Vector3 position within the radius.</returns>
    private Vector3 GenerateRandomPosition()
    {
        // Generate a random point within the given radius around the map center
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        Vector3 randomPosition = new Vector3(randomPoint.x, 0, randomPoint.y);

        // Shift the random point by the map center position to stay within bounds
        return mapCenter.position + randomPosition;
    }

    /// <summary>
    /// Sets the position of the target indicator to the newly chosen destination.
    /// </summary>
    /// <param name="position">The position of the new target destination.</param>
    private void SetTargetIndicatorPosition(Vector3 position)
    {
        if (targetIndicator != null)
        {
            targetIndicator.position = position;
        }
    }

    /// <summary>
    /// Updates the direction indicator to show the current movement direction of the object.
    /// Only updates if the object is moving.
    /// </summary>
    private void UpdateDirectionIndicator()
    {
        if (directionIndicator != null && agent.velocity.sqrMagnitude > 0.01f)
        {
            // Align the direction indicator with the current movement direction
            directionIndicator.forward = agent.velocity.normalized;
        }
    }
}
