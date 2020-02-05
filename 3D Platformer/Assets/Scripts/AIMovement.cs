using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField] private float acceleration = 0;
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float baseJumpSpeed = 10;
    [SerializeField] private float jumpSpeedBoost = 25;
    [SerializeField] private float stoppingDistance = 25;
    [SerializeField] private Transform target = null;
    private PlatformerPhysicSim ps = null;
    private Vector3 direction = Vector3.zero;
    private float distanceToTarget = 0;
    private float mass = 0;
    private float keyDownTime = 0;
    private float minKeyDownTime = 0.1f;
    private float maxKeyDownTime = 0.3f;
    private int jumpCount = 0;
    private float fixedDeltaTime = 0;


    private void Start()
    {
        if (!target) target = GameObject.FindGameObjectWithTag("Player").transform;
        ps = GetComponent<PlatformerPhysicSim>();
        mass = ps.Mass;
        fixedDeltaTime = Time.fixedDeltaTime;
    }
    
    private void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > stoppingDistance && ps.IsGrounded)
        {
            MoveTo(target.position);
        }
    }

    private void MoveTo(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
        //ps.AddForce(acceleration * mass * direction);
        ps.Velocity = acceleration * Time.deltaTime * direction;
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
}