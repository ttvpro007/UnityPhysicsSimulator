using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicSimulation : MonoBehaviour
{
    [SerializeField] float mass = 1;
    [SerializeField] float gravity = -9.8f;
    [Range(0.1f, 5f)]
    [SerializeField] float zeroTo100Time = 0;
    [SerializeField] float maxSpeed = 0;
    [SerializeField] float jumpHeight = 3;
    [Range(0f, 1f)]
    [SerializeField] float friction = 0;
    [SerializeField] bool useAirDrag = true;
    [Range(0f, 1f)]
    [SerializeField] float airFriction = 0;
    [SerializeField] float airDensity = 1.225f;
    [SerializeField] float relativeSurfaceArea = 0;
    private float deltaTime = 0;
    private float netAcceleration = 0;
    private float netVelocity = 0;
    private float netForceMagnitude = 0;
    private Vector3 netForce = Vector3.zero;
    private Vector3 moveForce = Vector3.zero;
    private float moveAcceleration = 0;
    private Vector3 frictionForce = Vector3.zero;
    private float airDrag = 0;
    private Vector3 normalForce = Vector3.zero;
    private Vector3 gForce = Vector3.zero;
    private Rigidbody rb = null;

    public bool isGrounded = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        deltaTime = Time.fixedDeltaTime;
        moveAcceleration = 100 / 3.6f / zeroTo100Time;
    }

    private void FixedUpdate()
    {
        // should update forces first before simulate forces
        UpdateForces();
        SimulateForces();
    }

    private void UpdateForces()
    {
        float netVelocitySquare = netVelocity * netVelocity;
        Vector3 moveDirection = GetMoveForce();

        if (isGrounded) normalForce = mass * -gravity * Vector3.up;
        else { airDrag = useAirDrag ? 0.5f * airDensity * netVelocitySquare * airFriction * relativeSurfaceArea : 0; }

        gForce = mass * gravity * Vector3.up;

        moveForce = mass * moveDirection * zeroTo100Time;

        if (Input.GetKeyDown(KeyCode.Space)) Jump();

        if (moveForce != Vector3.zero) frictionForce = gravity * mass * friction * moveDirection;


        netForce = gForce + normalForce + moveForce - frictionForce;

        netForceMagnitude = netForce.magnitude;
    }

    private void SimulateForces()
    {
        if (netVelocity <= maxSpeed)
        {
            netAcceleration = (netForceMagnitude - airDrag) / mass;
            netAcceleration = Mathf.Max(0, netAcceleration);
        }

        if (netAcceleration == 0 && isGrounded) netVelocity = 0;
        netVelocity += netAcceleration * deltaTime;
        if (isGrounded) netVelocity = Mathf.Min(maxSpeed, netVelocity);

        Vector3 newPos = (netVelocity * deltaTime + 0.5f * netAcceleration * deltaTime * deltaTime) * netForce.normalized;
        rb.MovePosition(transform.position + newPos);
    }

    private void Jump()
    {
        moveForce.y += jumpHeight;
    }

    private Vector3 GetMoveForce()
    {
        Vector3 force =
            Input.GetAxis("Horizontal") * transform.right
            + Input.GetAxis("Vertical") * transform.forward;

        force = force.normalized * mass * moveAcceleration + moveForce.y * Vector3.up;

        return force;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + netForce);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
            airDrag = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = false;
            normalForce = Vector3.zero;
        }
    }
}
