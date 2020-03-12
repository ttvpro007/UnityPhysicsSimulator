using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PairedPortal
{
    public class PortalCamera : MonoBehaviour
    {
        private Portal portal = null;
        private Portal linkedPortal = null;
        private Camera camera = null;
        private MeshRenderer screen = null;
        
        private Camera playerCamera = null;
        float portalsAngleDifference = 0f;

        public Camera CameraComponent { get { return camera; } }

        private void Start()
        {
            portal = GetComponentInParent<Portal>();
            linkedPortal = portal.LinkedPortal;
            camera = GetComponent<Camera>();
            screen = portal.Screen;

            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            portalsAngleDifference = Quaternion.Angle(portal.transform.rotation, linkedPortal.transform.rotation);
        }

        public void Render()
        {
            if (camera)

            screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            //transform.position = GetNewPosition();
            //transform.rotation = GetNewRotation();

            Matrix4x4 m = portal.transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
            transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

            camera.Render();

            screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private Quaternion GetNewRotation()
        {
            Quaternion portalsRotationDifference = Quaternion.AngleAxis(portalsAngleDifference, Vector3.up);
            Vector3 newCamDirection = portalsRotationDifference * playerCamera.transform.forward;
            return Quaternion.LookRotation(newCamDirection, Vector3.up);
        }

        private Vector3 GetNewPosition()
        {
            Vector3 playerOffsetFromOtherPortal = playerCamera.transform.position - linkedPortal.transform.position;
            return portal.transform.position + playerOffsetFromOtherPortal;
        }
    }
}