using PairedPortal;
using UnityEngine;

namespace PhysicsSimulation.Helper
{
    public class RaycastHitInfo : MonoBehaviour
    {
        [SerializeField] private Transform view = null;
        [SerializeField] private LayerMask hitLayer = 9;
        private Vector3 rand = Vector3.zero;
        private Vector3 direction = Vector3.zero;
        public Vector3 Direction { get { return direction; } }
        public Transform CamTransform { get { return view; } }

        public RaycastHit GetHit(float range)
        {
            RaycastHit hit;

            rand = Random.insideUnitCircle / range;
            direction = (view.forward + rand).normalized;

            if (Physics.Raycast(view.position, direction, out hit, range, hitLayer))
            {
                Debug.DrawLine(view.position, hit.point, Color.red, 2f);

                LinkedPortalInfo linkedPortalInfo = hit.transform.GetComponent<LinkedPortalInfo>();
                if (linkedPortalInfo)
                {
                    RaycastHitInfo linkedPortalCamRaycastInfo = linkedPortalInfo.hitInfo;
                    hit = linkedPortalCamRaycastInfo.GetHit(range);
                }
            }

            return hit;
        }

        public RaycastHit GetHit(float range, LayerMask hitLayer)
        {
            RaycastHit hit;

            rand = Random.insideUnitCircle / range;
            direction = (view.transform.forward + rand).normalized;

            if (Physics.Raycast(view.transform.position, direction, out hit, range, hitLayer))
            {
                Debug.DrawLine(view.transform.position, hit.point, Color.red, 2f);
                Debug.Log(name + " hit " + hit.transform.name);

                LinkedPortalInfo linkedPortalInfo = hit.transform.GetComponent<LinkedPortalInfo>();
                if (linkedPortalInfo)
                {
                    RaycastHitInfo linkedPortalCamRaycastInfo = linkedPortalInfo.hitInfo;
                    hit = linkedPortalCamRaycastInfo.GetHit(range, hitLayer);
                }
            }

            return hit;
        }

        public static bool HitWall(Transform body, out RaycastHit hit, float maxDistance, LayerMask wallLayer)
        {
            hit = new RaycastHit();
            
            // check forward
            if (Physics.Raycast(body.position, body.forward, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check forward up
            if (Physics.Raycast(body.position, body.forward + body.up, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check backward
            if (Physics.Raycast(body.position, -body.forward, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check backward up
            if (Physics.Raycast(body.position, -body.forward + body.up, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check right
            if (Physics.Raycast(body.position, body.right, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check right up
            if (Physics.Raycast(body.position, body.right + body.up, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check left
            if (Physics.Raycast(body.position, -body.right, out hit, maxDistance, wallLayer))
            {
                return true;
            }
            // check left up
            if (Physics.Raycast(body.position, -body.right + body.up, out hit, maxDistance, wallLayer))
            {
                return true;
            }

            return false;
        }

        public static bool HitWallForward(Transform body, out RaycastHit hit, float maxDistance, LayerMask wallLayer)
        {
            hit = new RaycastHit();

            // check forward
            if (Physics.Raycast(body.position, body.forward, out hit, maxDistance, wallLayer))
            {
                return true;
            }

            return false;
        }
    }
}