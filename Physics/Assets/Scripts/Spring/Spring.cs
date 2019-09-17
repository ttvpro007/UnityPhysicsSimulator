using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] LayerMask surface;
    [SerializeField] float trueLength;
    [SerializeField] float springConstant;
    [Range(0.1f, 50f)]
    [SerializeField] float dampingModifier;
    float displacement;
    float beginOfFrameDisplacement = 0;
    public float deltaDisplacement { get { return displacement - beginOfFrameDisplacement; } }
    public float currentLength;
    Vector3 gForce;
    Vector3 springForce;
    Vector3 dampingForce;
    Vector3 gravity;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravity = Physics.gravity;
        CalculateForces();
    }

    private void FixedUpdate()
    {
        CalculateForces();
    }

    private void CalculateForces()
    {
        // Setup variable to calculate deltaX
        beginOfFrameDisplacement = displacement;
        currentLength = GetDistanceToSurface();
        displacement = trueLength - currentLength;

        // Forces
        gForce = rb.mass * gravity;
        dampingForce = Vector3.up * (dampingModifier * deltaDisplacement / Time.deltaTime);
        springForce = transform.TransformVector(Vector3.up) * (springConstant * displacement + dampingForce.y);

        // Apply spring force
        rb.AddForce(springForce);
    }

    private float GetDistanceToSurface()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformVector(Vector3.down), out hit, Mathf.Infinity, surface))
        {
            Debug.DrawRay(transform.position, transform.TransformVector(Vector3.down) * hit.distance, Color.red);
            return hit.distance;
        }

        return -1;
    }
}
