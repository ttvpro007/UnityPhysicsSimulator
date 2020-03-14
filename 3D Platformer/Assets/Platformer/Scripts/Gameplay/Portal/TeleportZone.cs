using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player.Control;

namespace PairedPortal
{
    public class TeleportZone : MonoBehaviour
    {
        private Portal portal = null;
        private Portal linkedPortal = null;
        private PortalTraveller traveller = null;

        private float angleDifference = 0f;

        private void Start()
        {
            portal = GetComponentInParent<Portal>();
            linkedPortal = portal.LinkedPortal;

            angleDifference = Quaternion.Angle(transform.rotation, linkedPortal.transform.rotation);
        }

        public virtual void Teleport(Transform fromPortalTransform, Transform toPortalTransform, Matrix4x4 transformationMatrix, Movement playerMovement, MouseLook mouseLook)
        {
            if (!traveller) return;

            if (playerMovement && mouseLook)
            {
                Vector3 eulerRotation = transformationMatrix.rotation.eulerAngles;
                float deltaAngle = Mathf.DeltaAngle(mouseLook.YRotation, eulerRotation.y);
                mouseLook.RotateY(deltaAngle);
                playerMovement.transform.position = transformationMatrix.GetColumn(3);
                playerMovement.Velocity = toPortalTransform.TransformVector(fromPortalTransform.InverseTransformVector(playerMovement.Velocity));
            }
            else
            {
                traveller.transform.SetPositionAndRotation(transformationMatrix.GetColumn(3), transformationMatrix.rotation);
                Rigidbody rb = traveller.transform.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.velocity = toPortalTransform.TransformVector(fromPortalTransform.InverseTransformVector(rb.velocity));
                    rb.angularVelocity = toPortalTransform.TransformVector(fromPortalTransform.InverseTransformVector(rb.angularVelocity));
                }
            }

        }

        private Matrix4x4 GetPositionAndRotationMatrix(Transform travellerTransform, Transform fromPortalTransform, Transform toPortalTransform)
        {
            return toPortalTransform.localToWorldMatrix * fromPortalTransform.worldToLocalMatrix * travellerTransform.localToWorldMatrix;
        }

        private void HandleClipping(MeshRenderer meshRenderer)
        {
            Vector3 travellerPortalVector = transform.position - traveller.transform.position;
            float dot = Vector3.Dot(traveller.transform.forward, travellerPortalVector);

            if (dot > 0)
            {
                meshRenderer.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            traveller = other.GetComponent<PortalTraveller>();

            if (traveller && !traveller.Sender)
            {
                traveller.Sender = portal;
                //HandleClipping(linkedPortal.Screen);
                linkedPortal.Screen.enabled = false;
                Matrix4x4 m = GetPositionAndRotationMatrix(traveller.transform, portal.transform, linkedPortal.transform);
                Teleport(portal.transform, linkedPortal.transform, m, traveller.transform.GetComponent<Movement>(), traveller.transform.GetComponentInChildren<MouseLook>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!traveller)
            {
                traveller = other.GetComponent<PortalTraveller>();
            }
            else if (traveller.Sender.gameObject != portal.gameObject)
            {
                traveller.Sender = null;
                traveller = null;
                portal.Screen.enabled = true;
                
            }
        }
    }
}