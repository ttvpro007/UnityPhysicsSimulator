using UnityEngine;
using PhysicsSimulation;
using PhysicsSimulation.Helper;
using System;

namespace Player.Control
{
    public class Movement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float acceleration = 0f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float baseJumpSpeed = 10f;
        [SerializeField] private float jumpSpeedBoost = 25f;

        [Header("Slope Handling Settings")]
        [SerializeField] private float modelHeight = 0f;
        [SerializeField] private float maxTraversableSlopeAngle = 180f;
        [SerializeField] private LayerMask groundLayer = 8;

        [Header("Wall Handling Settings")]
        [SerializeField] private float distanceToWall = 2f;
        [SerializeField] private LayerMask wallLayer = 8;
        [SerializeField] private bool debug = false;

        private PlatformerPhysicsSim ps = null;
        private float mass = 0f;
        private float heightPadding = 0f;
        private float keyDownTime = 0f;
        private float minKeyDownTime = 0.1f;
        private float maxKeyDownTime = 0.3f;
        private int jumpCount = 0;
        private float fixedDeltaTime = 0f;
        private RaycastHit hit;
        private Vector3 moveDirection = Vector3.zero;
        private float slopeAngle = 0;

        public Vector3 Velocity { get { return ps.Velocity; } set { ps.Velocity = value; } }

        private void Start()
        {
            ps = GetComponent<PlatformerPhysicsSim>();
            mass = ps.Mass;
            heightPadding = ps.GroundDistance;
            fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update()
        {
            CalculateMoveDirection();

            Move();

            if (ps.IsGrounded)
            {
                jumpCount = 0;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                keyDownTime = 0;
                jumpCount++;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                Jump();
            }

            AdjustPosition();

            Debug();
        }

        private void AdjustPosition()
        {
            if (ps.IsGrounded)
            if (hit.distance < modelHeight)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up * (modelHeight + heightPadding), 5f * Time.deltaTime);
            }
        }

        private void CalculateMoveDirection()
        {
            moveDirection = GetInputMoveDirection();

            slopeAngle = CalculateSlopeAngle(out hit);
            if (slopeAngle > maxTraversableSlopeAngle + 90f || slopeAngle == 90f) return;

            if (moveDirection != Vector3.zero)
            {
                if (ps.IsGrounded)
                {
                    moveDirection = Vector3.Cross(hit.normal, -transform.right);
                }
                else
                {
                    if (RaycastHitInfo.HitWall(transform, out hit, distanceToWall, wallLayer))
                    {
                        moveDirection = PlatformerPhysicsSim.WallHorizontalParallelDirection(transform, hit.normal, moveDirection);
                    }
                }
            }
        }

        private float CalculateSlopeAngle(out RaycastHit hit)
        {
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, modelHeight + heightPadding, groundLayer))
            {
                return Vector3.Angle(transform.forward, hit.normal);
            }

            return 0f;
        }

        private Vector3 GetInputMoveDirection()
        {
            return Input.GetAxis("Horizontal") * transform.right
                                           + Input.GetAxis("Vertical") * transform.forward;
        }

        private void Move()
        {
            ps.AddForce(acceleration * mass * moveDirection);
            ps.Velocity = ClampVelocityXZ(ps.Velocity, maxSpeed);
        }

        private void Jump()
        {
            Vector3 newVelocity = ps.Velocity;

            // limited jump
            if (jumpCount < 1)
            {
                // has not pressed jump long enough
                if (keyDownTime < minKeyDownTime)
                {
                    newVelocity.y = baseJumpSpeed;
                    ps.Velocity = newVelocity;
                }
                // has not reached max key press time and has not reached max jump speed
                else if (keyDownTime < maxKeyDownTime && newVelocity.y < jumpSpeedBoost)
                {
                    newVelocity.y += jumpSpeedBoost * (1 - keyDownTime) * fixedDeltaTime;
                    newVelocity.y = Mathf.Min(newVelocity.y, jumpSpeedBoost);
                    ps.Velocity = newVelocity;
                }
            }
            else if (jumpCount < 2)
            {
                if (keyDownTime < minKeyDownTime)
                {
                    newVelocity.y = baseJumpSpeed;
                    ps.Velocity = newVelocity;
                }
            }

            keyDownTime += fixedDeltaTime;
        }

        private Vector3 ClampVelocityXZ(Vector3 velocity, float clampMagnitude)
        {
            // clamp speed on XZ plane
            Vector2 xz = new Vector2(velocity.x, velocity.z);
            xz = Vector2.ClampMagnitude(xz, clampMagnitude);
            velocity.x = xz.x;
            velocity.z = xz.y;

            return velocity;
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            this.moveDirection = moveDirection;
            ps.AddForce(acceleration * mass * moveDirection);
        }

        private void Debug()
        {
            if (!debug) return;

            UnityEngine.Debug.DrawLine(transform.position, transform.position + moveDirection * 2, Color.blue);
            UnityEngine.Debug.DrawLine(transform.position, transform.position - Vector3.up * 2, Color.green);
        }
    }
}