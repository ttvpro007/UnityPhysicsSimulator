using PhysicsSimulation;
using UnityEngine;

namespace AI
{
    public class AIMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 0;
        [SerializeField] private float maxSpeed = 10;
        [SerializeField] private float jumpSpeed = 10;
        private PlatformerPhysicsSim ps = null;
        private Vector3 direction = Vector3.zero;
        private float mass = 0;
        private float fixedDeltaTime = 0;

        public Vector3 Direction { get { return direction; } }
        public PlatformerPhysicsSim PhysicsSimulator { get { return ps; } }

        private void Start()
        {
            if (!ps) ps = GetComponent<PlatformerPhysicsSim>();
            mass = ps.Mass;
            fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Update()
        {
            //transform.LookAt(transform.position + direction);
        }

        public void MoveTo(Vector3 targetPosition)
        {
            direction = targetPosition - transform.position;
            direction.y = 0;
            direction.Normalize();

            //ps.AddForce(acceleration * mass * direction);
            ps.Velocity = acceleration * Time.deltaTime * direction;
            ps.Velocity = ClampVelocityXZ(ps.Velocity, maxSpeed);
        }

        public void Jump()
        {
            Vector3 jumpVelocity = ps.Velocity;
            jumpVelocity.y = jumpSpeed;
            ps.Velocity = jumpVelocity;
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
    }
}