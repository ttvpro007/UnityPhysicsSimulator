using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicSimulation : MonoBehaviour
{
    [SerializeField] float mass = 1;
    [SerializeField] float accelerationMagnitude = 0;
    [Range(0f, 1f)]
    [SerializeField] float friction = 0;
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
    private Vector3 frictionForce = Vector3.zero;
    private float airDrag = 0;
    private Vector3 normalForce = Vector3.zero;
    private float gravity = -9.8f;
    private Vector3 gForce = Vector3.zero;
    private Rigidbody rb = null;

    public bool isGrounded = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        deltaTime = Time.fixedDeltaTime;
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
        Vector3 moveDirection = GetMoveDirection();

        if (isGrounded) normalForce = mass * -gravity * Vector3.up;
        airDrag = 0.5f * airDensity * netVelocitySquare * airFriction * relativeSurfaceArea;
        gForce = mass * gravity * Vector3.up;
        moveForce = mass * moveDirection * accelerationMagnitude;
        frictionForce = gravity * mass * friction * -moveDirection;
        netForce = gForce + normalForce + moveForce + frictionForce;

        netForceMagnitude = netForce.magnitude;
    }

    private void SimulateForces()
    {
        netAcceleration = (netForceMagnitude - airDrag) / mass;
        netAcceleration = Mathf.Max(0, netAcceleration);
        if (netAcceleration == 0 && isGrounded) netVelocity = 0;
        netVelocity += netAcceleration * deltaTime;
        Vector3 newPos = (netVelocity * deltaTime + 0.5f * netAcceleration * deltaTime * deltaTime) * netForce.normalized;
        rb.MovePosition(transform.position + newPos);
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 moveDirection = 
            Input.GetAxisRaw("Horizontal") * transform.right
            + Input.GetAxisRaw("Vertical") * transform.forward;

        return moveDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
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
