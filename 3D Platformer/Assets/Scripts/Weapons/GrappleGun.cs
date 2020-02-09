using PhysicsSimulation;
using UnityEngine;
using PhysicsSimulation.Helper;

namespace Weapons
{
    public enum GrappleGunState
    {
        Hooked,
        Released
    }

    public class GrappleGun : MonoBehaviour
    {
        [SerializeField] private float range = 0;
        [SerializeField] private float stoppingDistance = 0;
        [SerializeField] private float constantSpeedDistance = 0;
        [SerializeField] private float distanceToAccelerationMult = 0;
        [SerializeField] private float bounceAngle = 0;
        [SerializeField] private float bounceSpeed = 0;
        [SerializeField] private float maxMoveSpeed = 0;
        [SerializeField] private float hookToTargetSpeed = 0;
        [SerializeField] private float wallCheckDistance = 2;
        [SerializeField] private LayerMask grappleLayer = 8;
        [SerializeField] private PlatformerPhysicsSim ps = null;

        private GrappleGunState state = GrappleGunState.Released;
        private RaycastHit hit;
        private RaycastHit wallHit;
        private RaycastHitInfo hitInfo = null;
        private bool isMouseDown = false;
        private bool canShootRay = true;
        private bool canMove = true;

        private float angleMultiplier = 0;
        private float minHeightForGrapple = 0;
        private float maxHeightForGrapple = 0;
        private Vector3 hookPoint = Vector3.zero;
        private Vector3 directionToHookPoint = Vector3.zero;
        private Vector3 moveDirection = Vector3.zero;
        private float distanceToHookPoint = 0;
        private float hookToTargetTime = 0;
        private float mouseDownTime = 0;

        // Start is called before the first frame update
        private void Start()
        {
            if (!hitInfo) hitInfo = GetComponentInParent<RaycastHitInfo>();
            if (!ps) ps = GetComponentInParent<PlatformerPhysicsSim>();
            angleMultiplier = Mathf.Tan(bounceAngle * Mathf.PI / 180);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                ShootRay();
            }

            if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
                Reset();
            }

            if (isMouseDown)
            {
                mouseDownTime += Time.deltaTime;

                //if (mouseDownTime >= 0)
                if (mouseDownTime >= hookToTargetTime && HitEdge())
                    state = GrappleGunState.Hooked;
            }

            switch (state)
            {
                case GrappleGunState.Hooked:
                    if (canMove)
                        MoveToward();
                    break;
                case GrappleGunState.Released:
                    break;
                default:
                    break;
            }
        }

        private bool HitEdge()
        {
            return hit.point.y < maxHeightForGrapple && hit.point.y > minHeightForGrapple;
        }

        private void MoveToward()
        {
            ps.UseGravity = false;

            if (hit.transform)
                distanceToHookPoint = Vector3.Distance(hit.transform.position, ps.transform.position);

            if (RaycastHitInfo.HitWall(ps.transform, out wallHit, wallCheckDistance, grappleLayer))
            {
                moveDirection = (angleMultiplier * Vector3.up - hit.normal).normalized;
                ps.Velocity = moveDirection * bounceSpeed;
                canMove = false;
                ps.UseGravity = true;
            }
            else
            {
                float speed = 0;
                directionToHookPoint = (hookPoint - ps.transform.position).normalized;

                if (distanceToHookPoint > constantSpeedDistance)
                {
                    speed = distanceToHookPoint * distanceToAccelerationMult;
                    ps.Velocity = speed * directionToHookPoint * Time.deltaTime;
                }
                else
                {
                    speed = constantSpeedDistance * distanceToAccelerationMult;
                    ps.Velocity = speed * directionToHookPoint * Time.deltaTime;
                }

                ps.Velocity = Vector3.ClampMagnitude(ps.Velocity, maxMoveSpeed);
            }
        }

        private void ShootRay()
        {
            if (canShootRay && IsRayHitObject(out hit, range, grappleLayer))
            {
                hookPoint = hit.point;
                directionToHookPoint = (hookPoint - ps.transform.position).normalized;
                distanceToHookPoint = hit.distance;
                hookToTargetTime = distanceToHookPoint / hookToTargetSpeed;
                canShootRay = false;

                minHeightForGrapple = hit.transform.position.y - hit.transform.localScale.y / 2;
                maxHeightForGrapple = hit.transform.position.y + hit.transform.localScale.y / 2;
            }
        }

        private bool IsRayHitObject(out RaycastHit hit, float range, LayerMask grappleLayer)
        {
            hit = hitInfo.GetHit(range, grappleLayer);
            return hit.transform != null;
        }

        /// <summary>
        /// Returns a forward perpendicular vector to the input direction as in pendulum motion
        /// </summary>
        private Vector3 GetMoveDirection(Vector3 toTargetDirection)
        {
            Vector3 proj = Vector3.ProjectOnPlane(toTargetDirection, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(proj, toTargetDirection).normalized;
            Vector3 direction = Vector3.Cross(toTargetDirection, right).normalized;
            return direction;
        }

        private void Reset()
        {
            ps.UseGravity = true;
            canShootRay = true;
            canMove = true;
            hookPoint = Vector3.zero;
            directionToHookPoint = Vector3.zero;
            moveDirection = Vector3.zero;
            distanceToHookPoint = 0;
            hookToTargetTime = 0;
            mouseDownTime = 0;
            state = GrappleGunState.Released;
        }
    }
}