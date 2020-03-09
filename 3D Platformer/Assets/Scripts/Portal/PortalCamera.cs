using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PairedPortal
{
    public class PortalCamera : MonoBehaviour
    {
        [SerializeField] private Transform playerCamera = null;
        [SerializeField] private Transform portal = null;
        [SerializeField] private Transform otherPortal = null;
        
        private void Update()
        {
            Vector3 playerOffsetFromOtherPortal = playerCamera.position - otherPortal.position;
            transform.position = portal.position + playerOffsetFromOtherPortal;

            float portalsAngleDifference = Quaternion.Angle(portal.rotation, otherPortal.rotation);
            Quaternion portalsRotationDifference = Quaternion.AngleAxis(portalsAngleDifference, Vector3.up);
            Vector3 newCamDirection = portalsRotationDifference * playerCamera.forward;
            transform.rotation = Quaternion.LookRotation(newCamDirection, Vector3.up);
        }
    }
}