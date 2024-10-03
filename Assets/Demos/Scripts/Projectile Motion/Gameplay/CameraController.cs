using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public List<Transform> pointsToTrack;
    public float smoothTime = 0.3f; // Smooth movement time for movement and drag
    public float zoomSpeed = 2.0f;  // Speed of zooming in/out
    public float minDistance = 10f; // Minimum distance of camera from center
    public float maxDistance = 100f; // Maximum distance of camera from center
    public float zoomPadding = 10f; // Padding to ensure all points fit comfortably
    public Vector2 minYMaxY = new Vector2(5f, 50f); // Minimum and maximum Y position of the camera
    public float dragSpeed = 5f; // Speed of camera movement during right-click drag

    public bool invertX = false; // Flag to invert horizontal movement
    public bool invertY = false; // Flag to invert vertical movement

    private Vector3 velocity = Vector3.zero; // For smooth movement
    private float yVelocity = 0f; // For smooth vertical movement
    private float currentDistance; // Current distance to look at target (center)

    private void LateUpdate()
    {
        if (pointsToTrack == null || pointsToTrack.Count == 0) return;

        // Calculate the center and the distance to the farthest point
        Vector3 center = CalculateCenterPoint();
        float maxDistanceToCenter = CalculateMaxDistance(center);

        // Move the camera smoothly to the correct position along the forward axis
        MoveCamera(center, maxDistanceToCenter);

        // Handle right-click drag to adjust position smoothly along a vertical and horizontal arc
        HandleMouseDrag(center);

        // Make the camera look at the center
        transform.LookAt(center);

        // Clamp the camera's Y position
        ClampYPosition();
    }

    // Move the camera along its forward axis to fit all points in view
    private void MoveCamera(Vector3 center, float maxDistanceToCenter)
    {
        // Calculate the target distance (using maxDistanceToCenter and zoomPadding)
        float requiredDistance = Mathf.Clamp(maxDistanceToCenter + zoomPadding, minDistance, maxDistance);
        currentDistance = requiredDistance; // Update the current distance

        // Target position is along the negative forward axis (away from the center)
        Vector3 direction = transform.forward * -1;
        Vector3 targetPosition = center + direction * requiredDistance;

        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Handle right-click drag to adjust position smoothly along a vertical and horizontal arc
    private void HandleMouseDrag(Vector3 center)
    {
        if (Input.GetMouseButton(1)) // Right mouse button is held down
        {
            // Get mouse movement along the X and Y axes
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Apply inversion flags
            mouseX *= invertX ? -1 : 1;
            mouseY *= invertY ? -1 : 1;

            // Calculate the current position relative to the center
            Vector3 directionToCamera = transform.position - center;
            float currentDistance = directionToCamera.magnitude; // Radius from center to camera

            // Convert to spherical coordinates to modify angles
            float currentElevationAngle = Mathf.Asin(directionToCamera.y / currentDistance); // Vertical angle in radians
            float currentAzimuthAngle = Mathf.Atan2(directionToCamera.z, directionToCamera.x); // Horizontal angle in radians

            // Update the elevation angle with the change from mouse input
            float elevationAngleChange = mouseY * dragSpeed * Time.deltaTime;
            float newElevationAngle = Mathf.Clamp(currentElevationAngle + elevationAngleChange, Mathf.Asin(minYMaxY.x / currentDistance), Mathf.Asin(minYMaxY.y / currentDistance));

            // Update the azimuth angle with the change from mouse input
            float azimuthAngleChange = mouseX * dragSpeed * Time.deltaTime;
            float newAzimuthAngle = currentAzimuthAngle + azimuthAngleChange;

            // Convert back to Cartesian coordinates
            float newY = Mathf.Sin(newElevationAngle) * currentDistance; // New height
            float horizontalDistance = Mathf.Cos(newElevationAngle) * currentDistance; // Radius on XZ plane

            // Calculate new X and Z positions based on the azimuth angle
            float newX = Mathf.Cos(newAzimuthAngle) * horizontalDistance;
            float newZ = Mathf.Sin(newAzimuthAngle) * horizontalDistance;

            // Calculate the new position in Cartesian coordinates
            Vector3 newPosition = new Vector3(newX, newY, newZ) + center;

            // Smoothly move the camera to the new position
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        }
    }

    // Method to clamp the Y position of the camera
    private void ClampYPosition()
    {
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, minYMaxY.x, minYMaxY.y), transform.position.z);
    }

    // Calculate the geometric center of all points
    private Vector3 CalculateCenterPoint()
    {
        if (pointsToTrack.Count == 1)
            return pointsToTrack[0].position;

        Vector3 sum = Vector3.zero;
        foreach (Transform point in pointsToTrack)
        {
            sum += point.position;
        }
        return sum / pointsToTrack.Count;
    }

    // Calculate the maximum distance from the center to the farthest point
    private float CalculateMaxDistance(Vector3 center)
    {
        float maxDistance = 0f;
        foreach (Transform point in pointsToTrack)
        {
            float distance = Vector3.Distance(center, point.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        return maxDistance;
    }
}
