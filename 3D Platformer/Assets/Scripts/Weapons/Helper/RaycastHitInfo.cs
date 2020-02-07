using UnityEngine;

namespace Weapons.Helper
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
            direction = (view.transform.forward + rand).normalized;

            if (Physics.Raycast(view.transform.position, direction, out hit, range, hitLayer))
            {
                Debug.DrawLine(view.transform.position, hit.point, Color.red, 2f);
            }

            return hit;
        }
    }
}