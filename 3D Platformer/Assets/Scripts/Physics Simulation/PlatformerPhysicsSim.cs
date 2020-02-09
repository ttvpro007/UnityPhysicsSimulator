using UnityEngine;

namespace PhysicsSimulation
{
    public class PlatformerPhysicsSim : MonoBehaviour
    {
        [SerializeField] private float mass = 1;
        [SerializeField] private float gravity = -9.8f;
        [Range(0f, 1f)]
        [SerializeField] private float friction = 1;
        [SerializeField] private Transform groundCheck = null;
        [SerializeField] private Transform ceillingCheck = null;
        [SerializeField] private float groundDistance = 0.2f;
        [SerializeField] private float ceillingDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer = 8;
        [SerializeField] private LayerMask ceillingLayer = 8;

        private bool isGrounded = true;
        private bool hitCeilling = true;
        private float acceleration = 0;
        private float deceleration = 0;
        private Vector3 netAcceleration = Vector3.zero;
        private Vector3 velocity = Vector3.zero;
        private Vector3 moveDirection = Vector3.zero;
        private Vector3 newPos = Vector3.zero;
        private Rigidbody rb = null;
        private float fixedDeltaTime = 0;
        private RaycastHit hit;

        public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
        public float Gravity { get { return gravity; } }
        public float Mass { get { return mass; } }
        public bool IsGrounded { get { return isGrounded; } }
        public bool IsHitWall { get { return isGrounded; } }

        public bool UseGravity { get; set; } = true;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            //acceleration = 100 / 3.6f / zeroToHundredTime;
            deceleration = gravity * friction;
            fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void FixedUpdate()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
            hitCeilling = Physics.CheckSphere(ceillingCheck.position, ceillingDistance, ceillingLayer);
#if UNITY_EDITOR
            deceleration = gravity * friction;
#endif
            netAcceleration = CalculateNetAcceleration();
            velocity += netAcceleration * fixedDeltaTime;

            if (isGrounded)
            {
                if (velocity.y < 0) velocity.y = 0;
                if (velocity.magnitude < 1f)
                {
                    netAcceleration = Vector3.zero;
                    velocity = Vector3.zero;
                }
            }

            Vector3 newPos = velocity * fixedDeltaTime;
            rb.MovePosition(transform.position + newPos);
        }

        private Vector3 CalculateNetAcceleration()
        {
            Vector3 netAcceleration = Vector3.zero;

            // on ground
            if (isGrounded)
            {
                // no input
                if (moveDirection == Vector3.zero)
                {
                    // still moving
                    if (velocity.magnitude > 0.1f)
                    {
                        netAcceleration = -deceleration * velocity.normalized;
                    }
                    else // not moving
                    {
                        netAcceleration = Vector3.zero;
                    }
                }
                else // has input
                {
                    netAcceleration = (acceleration - deceleration) * moveDirection;
                }
            }
            else // not on ground
            {
                if (UseGravity)
                    netAcceleration = (acceleration - deceleration) * moveDirection + gravity * Vector3.down;
                else
                    netAcceleration = Vector3.zero;

                if (hitCeilling)
                    velocity.y = 0;
            }

            return netAcceleration;
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            this.moveDirection = moveDirection;
        }

        public void AddForce(Vector3 force)
        {
            acceleration = force.magnitude / mass;
            moveDirection = force.normalized;
        }
    }
}