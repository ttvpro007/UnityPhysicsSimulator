using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastHitInfo : MonoBehaviour
{
    [SerializeField] private GameObject cam = null;
    [SerializeField] private LayerMask hitLayer = 9;
    private Vector3 rand = Vector3.zero;
    private Vector3 direction = Vector3.zero;
    public Vector3 Direction { get { return direction; } }

    public RaycastHit GetHit(float range)
    {
        RaycastHit hit;

        rand = Random.insideUnitCircle / range;
        direction = (cam.transform.forward + rand).normalized;

        if (Physics.Raycast(cam.transform.position, direction, out hit, range, hitLayer))
        {
            Debug.DrawLine(cam.transform.position, hit.point, Color.red, 2f);
        }

        return hit;
    }
}
