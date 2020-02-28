using UnityEngine;
using PhysicsSimulation;
using PhysicsSimulation.Helper;

namespace Player.Control
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 0;
        [SerializeField] private float maxSpeed = 10;
        [SerializeField] private float baseJumpSpeed = 10;
        [SerializeField] private float jumpSpeedBoost = 25;
        [SerializeField] private float distanceToWall = 2;
        [SerializeField] private LayerMask wallLayer = 8;
        private PlatformerPhysicsSim ps = null;
        private float mass = 0;
        private float keyDownTime = 0;
        private float minKeyDownTime = 0.1f;
        private float maxKeyDownTime = 0.3f;
        private int jumpCount = 0;
        private float fixedDeltaTime = 0;
        private RaycastHit hit;
        private Vector3 moveDirection = Vector3.zero;


        private void Start()
        {
            ps = GetComponent<PlatformerPhysicsSim>();
            mass = ps.Mass;
            fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update()
        {
            Debug.DrawLine(transform.position, (transform.position + moveDirection) * 5);
            moveDirection = Input.GetAxis("Horizontal") * transform.right
                               + Input.GetAxis("Vertical") * transform.forward;
            Debug.DrawLine(transform.position, transform.position + moveDirection, Color.red);
            if (RaycastHitInfo.HitWall(transform, out hit, distanceToWall, wallLayer))
            {
                moveDirection = PlatformerPhysicsSim.WallHorizontalParallelDirection(transform, hit.normal, moveDirection);
            }

            ps.AddForce(acceleration * mass * moveDirection);
            ps.Velocity = ClampVelocityXZ(ps.Velocity, maxSpeed);

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
    }
}