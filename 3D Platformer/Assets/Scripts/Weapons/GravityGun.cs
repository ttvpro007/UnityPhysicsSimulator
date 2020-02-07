using UnityEngine;
using Weapons.Helper;

namespace Weapons
{
    public enum GravityGunState
    {
        Unoccupied,
        Pulling
    }

    public class GravityGun : MonoBehaviour
    {
        [SerializeField] private float range = 0;
        [SerializeField] private float distanceToAccelerationMult = 0;
        [SerializeField] private float objectMaxSpeed = 0;
        [SerializeField] private float holdPointMoveSpeed = 0;
        [SerializeField] private float holdPointMouseScrollMoveSpeed = 0;
        [SerializeField] private Transform holdPoint = null;
        [SerializeField] private float tolerantRange = 0;
        [SerializeField] private ParticleSystem rayFX = null;
        private GravityGunState state = GravityGunState.Unoccupied;
        private RaycastHit hit;
        private RaycastHitInfo hitInfo = null;
        private Vector3 pullDirection = Vector3.zero;
        private Transform hitObjectTransform = null;
        private Rigidbody hitObjectRB = null;
        private Transform camTransform = null;
        private float baseDistance = 5;
        private float newDistance = 0;
        private float distanceFromHoldPointToObject = Mathf.Infinity;
        private bool isMouseDown = false;
        private bool canShootRay = true;

        private void Start()
        {
            if (!hitInfo) hitInfo = GetComponentInParent<RaycastHitInfo>();
            camTransform = hitInfo.CamTransform;
            newDistance = baseDistance;
        }

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
                if (HasObject())
                {
                    UpdateDistanceFromHoldPointToObject();
                    UpdatePullDirection();
                }

                CheckIfChangeHoldPointDistance();
            }

            state = CheckState(state);

            switch (state)
            {
                case GravityGunState.Unoccupied:
                    break;
                case GravityGunState.Pulling:
                    PullObject(pullDirection);
                    break;
            }
        }

        private void UpdateDistanceFromHoldPointToObject()
        {
            distanceFromHoldPointToObject = GetDistanceToHoldPoint(hitObjectTransform.position);
        }

        private void UpdatePullDirection()
        {
            pullDirection = (holdPoint.transform.position - hitObjectTransform.position).normalized;
        }

        private void CheckIfChangeHoldPointDistance()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                ChangeHoldPointDistance(-holdPointMoveSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                ChangeHoldPointDistance(holdPointMoveSpeed * Time.deltaTime);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                ChangeHoldPointDistance(Input.GetAxis("Mouse ScrollWheel") * holdPointMouseScrollMoveSpeed * Time.deltaTime);
            }
        }

        private void ChangeHoldPointDistance(float changedDistance)
        {
            newDistance += changedDistance;
            newDistance = Mathf.Max(newDistance, baseDistance);
            UpdateHoldPointPosition(newDistance);
        }

        private void UpdateHoldPointPosition(float newDistance)
        {
            holdPoint.position = camTransform.position + camTransform.forward * newDistance;
        }

        private GravityGunState CheckState(GravityGunState state)
        {
            switch (state)
            {
                case GravityGunState.Unoccupied:
                    if (isMouseDown && HasObject() && !IsObjectInRange())
                    {
                        return GravityGunState.Pulling;
                    }
                    else
                    {
                        return state;
                    }
                case GravityGunState.Pulling:
                    if (!isMouseDown)
                    {
                        return GravityGunState.Unoccupied;
                    }
                    else
                    {
                        return state;
                    }
                default:
                    return GravityGunState.Unoccupied;
            }
        }

        // This is where transform and rigidbody of object is set if has hit
        private void ShootRay()
        {
            if (canShootRay && IsRayHitObject(out hit, range))
            {
                hitObjectTransform = hit.transform;
                hitObjectRB = hit.rigidbody;
                canShootRay = false;
            }
        }

        private void PullObject(Vector3 direction)
        {
            hitObjectRB.useGravity = false;
            float acceleration = distanceFromHoldPointToObject * distanceToAccelerationMult;
            hitObjectRB.velocity = acceleration * direction;
            hitObjectRB.velocity = Vector3.ClampMagnitude(hitObjectRB.velocity, objectMaxSpeed);
        }

        private bool IsRayHitObject(out RaycastHit hit, float range)
        {
            hit = hitInfo.GetHit(range);
            return hit.transform != null;
        }

        private bool HasObject()
        {
            return hitObjectTransform != null;
        }

        private bool IsObjectInRange()
        {
            return distanceFromHoldPointToObject <= tolerantRange;
        }

        private float GetDistanceToHoldPoint(Vector3 objectPosition)
        {
            return Vector3.Distance(holdPoint.position, objectPosition);
        }

        private void Reset()
        {
            state = GravityGunState.Unoccupied;

            canShootRay = true;
            hitObjectTransform = null;

            if (hitObjectRB)
            {
                hitObjectRB.useGravity = true;
                hitObjectRB = null;
            }

            newDistance = baseDistance;
            UpdateHoldPointPosition(newDistance);
            distanceFromHoldPointToObject = Mathf.Infinity;
            pullDirection = Vector3.zero;
        }
    }
}