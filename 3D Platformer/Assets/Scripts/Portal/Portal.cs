using PhysicsSimulation;
using Player.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePortal
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private Portal linkedPortal = null;
        [SerializeField] private Camera portalCamera = null;
        [SerializeField] public Collider gateCollider = null;

        private void TeleportTraveller(Transform travellerTransform)
        {
            Vector3 portalToPlayer = travellerTransform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

            if (dotProduct < 0)
            {
                float rotationDiff = -Quaternion.Angle(transform.rotation, linkedPortal.transform.rotation);
                rotationDiff += 180;
                travellerTransform.Rotate(Vector3.up, rotationDiff);

                Vector3 positionOffset = Quaternion.Euler(0, rotationDiff, 0) * portalToPlayer;
                travellerTransform.position = linkedPortal.transform.position + positionOffset;

                // Toggle to promote raycast hit from this portal camera and prevent raycast hit from other portal camera
                gateCollider.enabled = false;
                linkedPortal.gateCollider.enabled = true;
            }
        }

        private void TeleportTraveller(Portal fromPortal, Portal toPortal, PortalTraveller traveller)
        {
            Vector3 portalToPlayer = traveller.transform.position - fromPortal.transform.position;
            float dotProduct = Vector3.Dot(fromPortal.transform.forward, portalToPlayer);

            if (dotProduct > 0)
            {
                Debug.DrawLine(fromPortal.transform.position, fromPortal.transform.position + portalToPlayer, Color.green, 10);
                Vector3 reflectionNormal = Vector3.ProjectOnPlane(portalToPlayer, transform.forward).normalized;
                Debug.DrawLine(fromPortal.transform.position, fromPortal.transform.position + reflectionNormal, Color.red, 10);
                Vector3 newPositionOffset = Vector3.Reflect(-portalToPlayer, reflectionNormal);
                Debug.DrawLine(fromPortal.transform.position, fromPortal.transform.position + newPositionOffset, Color.blue, 10);
                traveller.transform.position = toPortal.transform.position + newPositionOffset;

                PlatformerPhysicsSim ps = traveller.transform.GetComponent<PlatformerPhysicsSim>();
                if (ps)
                {
                    ps.Velocity = Quaternion.FromToRotation(ps.Velocity, -toPortal.transform.forward) * ps.Velocity;
                    float rotationOffset = Quaternion.Angle(fromPortal.transform.rotation, toPortal.transform.rotation);
                    traveller.transform.GetComponentInChildren<MouseLook>().Rotate(traveller.transform.up, rotationOffset);
                }
                //else
                //{
                //    Quaternion rotation = Quaternion.FromToRotation()
                //}

                traveller.Sender = this;
                traveller.IsTravelling = true;

                // Toggle to promote raycast hit from this portal camera and prevent raycast hit from other portal camera
                gateCollider.enabled = false;
                linkedPortal.gateCollider.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PortalTraveller traveller = other.transform.GetComponent<PortalTraveller>();
            if (traveller)
            {
                if (!traveller.Sender || !traveller.IsTravelling)
                {
                    TeleportTraveller(this, linkedPortal, traveller);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PortalTraveller traveller = other.transform.GetComponent<PortalTraveller>();
            if (traveller)
            {
                if (traveller.Sender != this)
                {
                    traveller.Sender = null;
                    traveller.IsTravelling = false;
                }
            }
        }

        public static bool IsVisibleFromCamera(Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
    }
}