using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public List<Transform> pointsToTrack;
    public float smoothTime = 0.3f; // Smooth movement time
    public float zoomSpeed = 2.0f;  // Speed of zooming in/out
    public float minDistance = 10f; // Minimum distance of camera from center
    public float maxDistance = 100f; // Maximum distance of camera from center
    public float zoomPadding = 10f; // Padding to ensure all points fit comfortably

    private Vector3 velocity = Vector3.zero; // For smooth movement

    private void LateUpdate()
    {
        if (pointsToTrack == null || pointsToTrack.Count == 0) return;

        // Calculate the center and the distance to the farthest point
        Vector3 center = CalculateCenterPoint();
        float maxDistanceToCenter = CalculateMaxDistance(center);

        // Move the camera smoothly to the correct position along the forward axis
        MoveCamera(center, maxDistanceToCenter);

        // Make the camera look at the center
        transform.LookAt(center);
    }

    // Move the camera along its forward axis to fit all points in view
    private void MoveCamera(Vector3 center, float maxDistanceToCenter)
    {
        // Calculate the target distance (using maxDistanceToCenter and zoomPadding)
        float requiredDistance = Mathf.Clamp(maxDistanceToCenter + zoomPadding, minDistance, maxDistance);

        // Target position is along the negative forward axis (away from the center)
        Vector3 direction = transform.forward * -1;
        Vector3 targetPosition = center + direction * requiredDistance;

        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
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