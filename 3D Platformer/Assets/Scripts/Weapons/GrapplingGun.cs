using PhysicsSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons.Helper;

namespace Weapons
{
    public enum GrapplingGunState
    {
        Hooked,
        Released
    }

    public class GrapplingGun : MonoBehaviour
    {
        [SerializeField] private float range = 0;
        [SerializeField] private float moveSpeed = 0;
        [SerializeField] private float hookToTargetSpeed = 0;
        [SerializeField] private LayerMask grappleLayer = 8;
        [SerializeField] private PlatformerPhysicsSim ps = null;

        private GrapplingGunState state = GrapplingGunState.Released;
        private RaycastHit hit;
        private RaycastHitInfo hitInfo = null;
        private bool isMouseDown = false;
        private bool canShootRay = true;

        private float angle = 0;
        private float acceleration = 0;
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
            //if (!ps) ps = GameObject.FindGameObjectWithTag("Player").GetComponent<PlatformerPhysicsSim>();
            if (!ps) ps = GetComponentInParent<PlatformerPhysicsSim>();
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

                if (mouseDownTime >= 0)
                //if (mouseDownTime >= hookToTargetTime)
                    state = GrapplingGunState.Hooked;
            }

            switch (state)
            {
                case GrapplingGunState.Hooked:
                    //MoveToward();
                    Swing();
                    break;
                case GrapplingGunState.Released:
                    break;
                default:
                    break;
            }
        }

        private void Swing()
        {
            ps.UseGravity = false;
            angle = Vector3.Angle(transform.position - hookPoint, Vector3.down);
            acceleration += 9.8f / distanceToHookPoint * Mathf.Sin(angle * Mathf.PI / 180);
            directionToHookPoint = hookPoint - transform.position;
            moveDirection = GetMoveDirection(directionToHookPoint);
            MoveToward();
        }

        private void MoveToward()
        {
            ps.Velocity = acceleration * moveDirection * Time.deltaTime;
            //ps.Velocity = moveSpeed * moveDirection * Time.deltaTime;
            //ps.Velocity = moveSpeed * directionToHookPoint * Time.deltaTime;
            ps.Velocity = Vector3.ClampMagnitude(ps.Velocity, 1000);
        }

        private void ShootRay()
        {
            if (canShootRay && IsRayHitObject(out hit, range, grappleLayer))
            {
                hookPoint = hit.point;
                directionToHookPoint = (hookPoint - transform.position).normalized;
                //distanceToHookPoint = Vector3.Distance(transform.position, hookPoint);
                distanceToHookPoint = hit.distance;
                hookToTargetTime = distanceToHookPoint / hookToTargetSpeed;
                canShootRay = false;
            }
        }

        private bool IsRayHitObject(out RaycastHit hit, float range, LayerMask grappleLayer)
        {
            hit = hitInfo.GetHit(range, grappleLayer);
            return hit.transform != null;
        }

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
            angle = 0;
            acceleration = 0;
            canShootRay = true;
            hookPoint = Vector3.zero;
            directionToHookPoint = Vector3.zero;
            moveDirection = Vector3.zero;
            distanceToHookPoint = 0;
            hookToTargetTime = 0;
            mouseDownTime = 0;
            state = GrapplingGunState.Released;
        }
    }
}