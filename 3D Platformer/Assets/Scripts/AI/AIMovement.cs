using PhysicsSimulation;
using PhysicsSimulation.Helper;
using UnityEngine;

namespace AI
{
    public class AIMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 0;
        [SerializeField] private float maxSpeed = 10;
        [SerializeField] private float jumpSpeed = 10;
        [SerializeField] private float knockBackSpeed = 20;
        [SerializeField] private float distanceToWall = 2;
        [Range(0f, 1f)]
        [SerializeField] private float wallVelocityDampener = 1;
        [SerializeField] private LayerMask wallLayer = 8;
        [SerializeField] private LayerMask objectLayer = 8;
        private PlatformerPhysicsSim ps = null;
        private Vector3 direction = Vector3.zero;
        private RaycastHit hit;
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

        public void KnockBack(Vector3 direction, bool isJumpBack)
        {
            Vector3 knockbackVelocity = direction * knockBackSpeed;
            knockbackVelocity.y = isJumpBack ? knockBackSpeed : 0;
            ps.Velocity = knockbackVelocity;
        }

        private void BounceOffWall()
        {
            if (RaycastHitInfo.HitWall(transform, out hit, distanceToWall, wallLayer))
            {
                ps.Velocity = Vector3.Reflect(ps.Velocity, hit.normal) * wallVelocityDampener;
            }
        }

        private void BounceOffObject(Vector3 objectVelocity)
        {
            ps.Velocity += objectVelocity;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                KnockBack((transform.position - other.transform.position).normalized, true);
            }

            if (((1 << other.gameObject.layer) & wallLayer) != 0)
            {
                BounceOffWall();
            }

            if (((1 << other.gameObject.layer) & objectLayer) != 0)
            {
                BounceOffObject(other.GetComponent<Rigidbody>().velocity);
            }
        }
    }
}