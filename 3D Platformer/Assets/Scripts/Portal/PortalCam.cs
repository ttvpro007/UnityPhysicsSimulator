using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCam : MonoBehaviour
{
    [SerializeField] private Transform playerCamera = null;
    [SerializeField] private Transform portal = null;
    [SerializeField] private Transform otherPortal = null;
    [SerializeField] private MeshRenderer renderedWorld = null;
    [SerializeField] private float FOV = 66;
    public float dotDirection = 0;
    public float dotPosition = 0;
    public float cosFOV = 0;

    private void Start()
    {
        cosFOV = Mathf.Cos(FOV * Mathf.PI / 180);
    }

    private void LateUpdate()
    {
        renderedWorld.enabled = ShouldEnable();
        Matrix4x4 transformationMatrix = portal.localToWorldMatrix * otherPortal.worldToLocalMatrix * playerCamera.localToWorldMatrix;
        transform.SetPositionAndRotation(transformationMatrix.GetColumn(3), transformationMatrix.rotation);
    }

    private bool ShouldEnable()
    {
        return 
            !IsOpposite(renderedWorld.transform.up, transform.forward) && 
            !IsWithinFOV(renderedWorld.transform.up, (transform.position - renderedWorld.transform.position).normalized, cosFOV);
    }

    public bool IsOpposite(Vector3 forward, Vector3 direction)
    {
        dotDirection = Vector3.Dot(forward, direction);
        return dotDirection < 0;
    }

    public bool IsWithinFOV(Vector3 forward, Vector3 direction, float cosFOV)
    {
        dotPosition = Vector3.Dot(forward, direction);
        return dotPosition > cosFOV;
    }
}
