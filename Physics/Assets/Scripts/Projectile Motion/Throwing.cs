using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwing : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject grenade;
    [SerializeField] float throwingAngle;
    [SerializeField] float gravity;
    const float SpeedModifier = 4.4f;

    float initialVelocityY;
    float initialVelocityX;
    float theta;
    float tanTheta;

    public float InitialThrowingVelocity
    {
        get { return CalculateInitialVelocity(); }
    }

    public float Gravity
    {
        get { return gravity; }
    }

    public float Theta
    {
        get { return throwingAngle * Mathf.PI / 180f; }
    }

    public float TurningAngle
    {
        get { return transform.eulerAngles.y; }
    }

    public float DistanceToTarget
    {
        get { return Vector3.Distance(transform.position, target.position); }
    }

    void Start()
    {
        Initiate();
    }

    private void Initiate()
    {
        if (!target) Debug.Log("No reference to target");
        if (!grenade) Debug.Log("No reference to grenade");
        if (initialVelocityX == 0) initialVelocityX = 5f;
        if (gravity == 0) gravity = 9.8f;
        if (throwingAngle == 0) throwingAngle = 45f;

        theta = Theta;
        tanTheta = Mathf.Tan(theta);
    }

    private void Update()
    {
        if (!target) return;
        if (!grenade) return;
        
        transform.LookAt(target);

        if (Input.GetMouseButtonDown(0))
        {
            Throw();
        }
    }

    private void Throw()
    {
        Instantiate(grenade, spawnPoint.position, Quaternion.identity);
    }

    private float CalculateInitialVelocity()
    {
        float initialVelocity = 0f;

        //initialVelocityX = DistanceToTarget / timeToTarget;
        //initialVelocityY = initialVelocityX * tanTheta;
        //initialVelocity = Mathf.Sqrt(initialVelocityX * initialVelocityX + initialVelocityY * initialVelocityY);

        if(throwingAngle % 90 != 0)
            initialVelocity = Mathf.Sqrt(DistanceToTarget * gravity / Mathf.Sin(2 * Theta));

        return initialVelocity / SpeedModifier;
    }
}