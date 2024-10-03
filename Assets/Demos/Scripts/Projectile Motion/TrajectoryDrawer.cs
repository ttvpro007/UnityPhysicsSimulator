using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryDrawer : MonoBehaviour
{
    [Header("Trajectory Settings")]
    /// <summary>
    /// Number of points used to draw the trajectory.
    /// Higher values result in a smoother trajectory but may impact performance.
    /// </summary>
    [Tooltip("Number of points used to draw the trajectory. Higher values result in a smoother trajectory but may impact performance.")]
    [SerializeField] private int trajectoryResolution = 30;

    /// <summary>
    /// Layers that the trajectory should consider for collisions (e.g., ground, walls, obstacles).
    /// </summary>
    [Tooltip("Layers that the trajectory should consider for collisions (e.g., ground, walls, obstacles).")]
    [SerializeField] private LayerMask collisionLayers;

    [Header("Hit Point Settings")]
    /// <summary>
    /// Transform that will be positioned at the hit point when the trajectory collides with an object.
    /// </summary>
    [Tooltip("Transform that will be positioned at the hit point when the trajectory collides with an object.")]
    [SerializeField] private Transform hitPointTransform;

    [Header("Line Renderer Settings")]
    /// <summary>
    /// Gradient to control the color of the trajectory over time.
    /// </summary>
    [Tooltip("Gradient to control the color of the trajectory over time.")]
    [SerializeField] private Gradient gradient;

    // Reference to the LineRenderer component used to draw the trajectory
    private LineRenderer lineRenderer;

    // Cached parameters to avoid unnecessary trajectory recalculations
    private Vector3 lastStartPosition;
    private Vector3 lastInitialVelocity;
    private Vector3 lastGravity;

    public Vector3 HitPointPosition { get; private set; }

    public List<Vector3> TrajectoryPath { get; private set; } = new();

    private void Start()
    {
        // Get the LineRenderer component attached to the same GameObject
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Assign default material if none is set
        lineRenderer.material ??= new Material(Shader.Find("Sprites/Default"));

        // Set the gradient for the LineRenderer
        lineRenderer.colorGradient = gradient;
    }

    /// <summary>
    /// Draws the trajectory based on the starting position, initial velocity, gravity, and time step.
    /// Only recalculates the trajectory if any of these parameters have changed since the last update.
    /// </summary>
    /// <param name="startPosition">The starting position of the trajectory.</param>
    /// <param name="initialVelocity">The initial velocity of the object following the trajectory.</param>
    /// <param name="timeStep">The time interval between each step in the trajectory.</param>
    /// <param name="gravity">The gravitational force affecting the trajectory.</param>
    public void DrawTrajectory(Vector3 startPosition, Vector3 initialVelocity, float timeStep, Vector3 gravity)
    {
        // Check if the parameters have changed since the last calculation
        if (CheckParameterChanged(startPosition, initialVelocity, gravity))
        {
            // Clear trajectory collections
            ClearTrajectory();

            // Cache the current parameters if there is a change
            CacheParameters(startPosition, initialVelocity, gravity);

            // Ensure that the LineRenderer has enough points for the trajectory
            lineRenderer.positionCount = trajectoryResolution;

            // Set the first point of the trajectory at the start position
            lineRenderer.SetPosition(0, startPosition);
            TrajectoryPath.Add(startPosition);

            Vector3 previousPosition = startPosition;  // The starting point for trajectory calculation

            // Iterate through each point in the trajectory
            for (int i = 1; i < trajectoryResolution; i++)
            {
                // Calculate the time for this segment of the trajectory
                float simulationTime = (i / (float)trajectoryResolution) * timeStep;

                // Calculate the current position of the object using the projectile motion formula
                Vector3 currentPosition = startPosition +
                                          (initialVelocity * simulationTime) +
                                          (0.5f * simulationTime * simulationTime * gravity);

                // Check if the trajectory intersects with any objects on the specified collision layers
                if (Physics.Raycast(previousPosition, currentPosition - previousPosition, out RaycastHit hit, Vector3.Distance(previousPosition, currentPosition), collisionLayers))
                {
                    // If a collision is detected, stop drawing the trajectory at the collision point
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    TrajectoryPath.Add(hit.point);

                    // Update the hit point position and rotation to match the collision
                    SetHitPointTransform(hit);
                    break;
                }

                // Set the position of the current point in the LineRenderer
                lineRenderer.SetPosition(i, currentPosition);
                TrajectoryPath.Add(currentPosition);

                // Update the previous position to the current one for the next iteration
                previousPosition = currentPosition;
            }
        }
    }

    /// <summary>
    /// Clears the trajectory by resetting the LineRenderer's point count to zero.
    /// This removes any previously drawn trajectory from the screen.
    /// </summary>
    public void ClearTrajectory()
    {
        lineRenderer.positionCount = 0;
        TrajectoryPath.Clear();
    }

    /// <summary>
    /// Caches the current trajectory parameters for comparison in future calculations.
    /// </summary>
    /// <param name="startPosition">The starting position of the trajectory.</param>
    /// <param name="initialVelocity">The initial velocity of the trajectory.</param>
    /// <param name="gravity">The gravitational force affecting the trajectory.</param>
    private void CacheParameters(Vector3 startPosition, Vector3 initialVelocity, Vector3 gravity)
    {
        lastStartPosition = startPosition;
        lastInitialVelocity = initialVelocity;
        lastGravity = gravity;
    }

    /// <summary>
    /// Checks if any of the trajectory parameters (start position, velocity, or gravity) have changed.
    /// If they have changed, the trajectory will be recalculated.
    /// </summary>
    /// <param name="startPosition">The current starting position.</param>
    /// <param name="initialVelocity">The current initial velocity.</param>
    /// <param name="gravity">The current gravitational force.</param>
    /// <returns>True if any parameters have changed, otherwise false.</returns>
    private bool CheckParameterChanged(Vector3 startPosition, Vector3 initialVelocity, Vector3 gravity)
    {
        return startPosition != lastStartPosition || initialVelocity != lastInitialVelocity || gravity != lastGravity;
    }

    /// <summary>
    /// Sets the hit point transform (position and rotation) to match the point where the trajectory collides with an object.
    /// The hit point will be aligned with the normal of the surface it hits.
    /// </summary>
    /// <param name="hit">The RaycastHit object containing information about the collision.</param>
    private void SetHitPointTransform(RaycastHit hit)
    {
        HitPointPosition = hit.point;

        if (hitPointTransform)
        {
            // Set the hitPointTransform position to the hit location and align its up axis with the surface normal
            hitPointTransform.SetPositionAndRotation(HitPointPosition, Quaternion.FromToRotation(hitPointTransform.up, hit.normal) * hitPointTransform.rotation);
        }
    }
}
