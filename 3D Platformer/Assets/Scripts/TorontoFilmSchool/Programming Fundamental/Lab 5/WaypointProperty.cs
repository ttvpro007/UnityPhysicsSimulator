using UnityEngine;

public class WaypointProperty : MonoBehaviour
{
    const float waypointGizmoRadius = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, waypointGizmoRadius);
    }
}
