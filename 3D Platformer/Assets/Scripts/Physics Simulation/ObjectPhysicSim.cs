using UnityEngine;

namespace PhysicsSimulation
{
    public class ObjectPhysicSim : MonoBehaviour
    {
        [SerializeField] private float gravity = -9.8f;

        private bool isGrounded = true;
        private float acceleration = 0;
        private float deceleration = 0;
        private Vector3 netAcceleration = Vector3.zero;
        private Vector3 velocity = Vector3.zero;
        private Vector3 moveDirection = Vector3.zero;
        private Rigidbody rb = null;
        private float fixedDeltaTime = 0;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (isGrounded)
            {
                netAcceleration = (acceleration + deceleration) * moveDirection;
            }
            else
            {
                netAcceleration = gravity * Vector3.up;
            }
        }
    }
}