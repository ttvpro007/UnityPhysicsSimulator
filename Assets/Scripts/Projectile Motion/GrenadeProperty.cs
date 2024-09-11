using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProperty : MonoBehaviour
{
    [SerializeField] float lifeTime;

    float gravity;
    float initialVelocity;
    float initialVelocityX;
    float initialVelocityY;
    float initialVelocityZ;
    float displacementX; // z axis on 3D system
    float displacementY; // y axis on 2D system
    float displacementZ; // x axis on 2D system
    float time = 0f;
    Rigidbody rb;
    Throwing thrower;

    float alpha;
    float beta;
    float theta;
    float cosAlpha;
    float cosBeta;
    float cosTheta;
    float sinTheta;
    
    const float frameTime = 1f / 30f;
    float timeSinceLastFrame = Mathf.Infinity;

    private void Start()
    {
        if (lifeTime == 0) lifeTime = 10f;

        Destroy(gameObject, lifeTime);

        rb = GetComponent<Rigidbody>();

        thrower = FindObjectOfType<Throwing>();

        gravity = thrower.Gravity;
        time = 0f;

        initialVelocity = thrower.InitialThrowingVelocity;

        // convert deg to rad
        alpha = thrower.TurningAngle * Mathf.PI / 180f;
        beta = (90 - thrower.TurningAngle) * Mathf.PI / 180f;
        theta = thrower.Theta;

        cosAlpha = Mathf.Cos(alpha);
        cosBeta = Mathf.Cos(beta);
        cosTheta = Mathf.Cos(theta);
        sinTheta = Mathf.Sin(theta);

        initialVelocityX = initialVelocity * cosTheta * cosBeta;
        initialVelocityY = initialVelocity * sinTheta;
        initialVelocityZ = initialVelocity * cosTheta * cosAlpha;
    }


    // physics logic execution
    private void FixedUpdate()
    {
        if (timeSinceLastFrame > frameTime)
        {
            UpdateDisplacement();
            timeSinceLastFrame = 0f;
        }

        timeSinceLastFrame += Time.deltaTime;
    }

    // position                 // rotation
    // x left right             // x rotates up down
    // y up down                // y rotates left right
    // z forward backward       // z twist
    private void UpdateDisplacement()
    {
        time += frameTime;

        // changes xz
        displacementZ = transform.position.z + initialVelocityZ * time;
        displacementX = transform.position.x + initialVelocityX * time;

        // changes y
        displacementY = transform.position.y + (initialVelocityY * time) - (0.5f * gravity * time * time);

        transform.position = new Vector3(displacementX, displacementY, displacementZ);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}