using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PairedPortal
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private Portal linkedPortal = null;
        [SerializeField] private Camera portalCamera = null;

        private float angleDifference = 0f;
        private PortalTraveller traveller = null;

        public Portal LinkedPortal { get { return linkedPortal; } }
        public Camera PortalCamera { get { return portalCamera; } }

        private void Start()
        {
            angleDifference = Quaternion.Angle(transform.rotation, linkedPortal.transform.rotation);
        }

        public void Teleport(PortalTraveller traveller)
        {
            Vector3 portalToTravellerOffset = traveller.transform.position - transform.position;
            traveller.transform.position = linkedPortal.transform.position + Quaternion.AngleAxis(angleDifference + 180, Vector3.up) * portalToTravellerOffset;
        }

        private void OnTriggerEnter(Collider other)
        {
            traveller = other.GetComponent<PortalTraveller>();

            if (traveller && !traveller.Sender)
            {
                traveller.Sender = this;
                Teleport(traveller);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!traveller)
            {
                traveller = other.GetComponent<PortalTraveller>();
            }
            else if (traveller.Sender.gameObject != gameObject)
            {
                traveller.Sender = null;
                traveller = null;
            }
        }
    }
}