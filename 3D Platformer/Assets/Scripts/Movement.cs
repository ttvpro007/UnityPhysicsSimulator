using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float acceleration = 0;
    [SerializeField] private float baseJumpSpeed = 10;
    [SerializeField] private float jumpSpeedBoost = 25;
    private PlatformerPhysicSim ps = null;
    private float mass = 0;
    private float keyDownTime = 0;
    private float minKeyDownTime = 0.1f;
    private float maxKeyDownTime = 0.3f;
    private int jumpCount = 0;
    private float airTime = 0;
    private float fixedDeltaTime = 0;


    private void Start()
    {
        ps = GetComponent<PlatformerPhysicSim>();
        mass = ps.Mass;
        fixedDeltaTime = Time.fixedDeltaTime;
    }
    
    private void Update()
    {
        Vector3 direction = Input.GetAxis("Horizontal") * transform.right
                            + Input.GetAxis("Vertical") * transform.forward;

        //ps.SetMoveDirection(direction.normalized);
        ps.AddForce(acceleration * mass * direction);

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

        keyDownTime += fixedDeltaTime;
    }
}
