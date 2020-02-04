using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State
{
    Unoccupied,
    Pulling
}

public class GravityGun : MonoBehaviour
{
    [SerializeField] private float range = 0;
    [SerializeField] private float distanceToAccelerationMult = 0;
    [SerializeField] private float objectMaxSpeed = 0;
    [SerializeField] private Transform holdPoint = null;
    [SerializeField] private float tolerantRange = 0;
    [SerializeField] private ParticleSystem rayFX = null;
    private Vector3 shootDirection = Vector3.zero;
    private State state = State.Unoccupied;
    private RaycastHit hit;
    private Transform hitObjectTransform = null;
    private Rigidbody hitObjectRB = null;
    private Vector3 pullDirection = Vector3.zero;
    private RaycastHitInfo hitInfo = null;
    private float distanceFromHoldPointToObject = Mathf.Infinity;
    private Vector3 lerpPos = Vector3.zero;
    private bool isMouseDown = false; 
    private bool canShootRay = true; 

    private void Start()
    {
        if (!hitInfo) hitInfo = GetComponentInParent<RaycastHitInfo>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            ShootRay();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
            Reset();
        }

        if (isMouseDown)
        {
            if (HasObject())
            {
                distanceFromHoldPointToObject = GetDistanceToHoldPoint(hitObjectTransform.position);
                pullDirection = (holdPoint.transform.position - hitObjectTransform.position).normalized;
            }
        }
        else
        {
            distanceFromHoldPointToObject = Mathf.Infinity;
            pullDirection = Vector3.zero;
        }

        state = CheckState(state);

        switch (state)
        {
            case State.Unoccupied:
                break;
            case State.Pulling:
                PullObject(pullDirection);
                break;
        }
    }

    private State CheckState(State state)
    {
        switch (state)
        {
            case State.Unoccupied:
                if (isMouseDown && HasObject() && !IsObjectInRange())
                {
                    return State.Pulling;
                }
                else
                {
                    return state;
                }
            case State.Pulling:
                if (!isMouseDown)
                {
                    return State.Unoccupied;
                }
                else
                {
                    return state;
                }
            default:
                return State.Unoccupied;
        }
    }

    // This is where transform and rigidbody of object is set if has hit
    private void ShootRay()
    {
        if (canShootRay && IsRayHitObject(out hit, range))
        {
            hitObjectTransform = hit.transform;
            hitObjectRB = hit.rigidbody;
            canShootRay = false;
        }
    }

    private void PullObject(Vector3 direction)
    {
        hitObjectRB.useGravity = false; 
        //float force = distanceFromHoldPointToObject * distanceToForceMult;
        //hitObjectRB.AddForce(force * direction);
        float acceleration = distanceFromHoldPointToObject * distanceToAccelerationMult;
        hitObjectRB.velocity = acceleration * direction;
        hitObjectRB.velocity = Vector3.ClampMagnitude(hitObjectRB.velocity, objectMaxSpeed);

        if (IsObjectInRange())
        {
            hitObjectRB.MovePosition(holdPoint.position);
        }
    }

    private bool IsRayHitObject(out RaycastHit hit, float range)
    {
        hit = hitInfo.GetHit(range);
        return hit.transform != null;
    }

    private bool HasObject()
    {
        return hitObjectTransform != null;
    }

    private bool IsObjectInRange()
    {
        return distanceFromHoldPointToObject <= tolerantRange;
    }

    private float GetDistanceToHoldPoint(Vector3 objectPosition)
    {
        return Vector3.Distance(holdPoint.position, objectPosition);
    }

    private void Reset()
    {
        canShootRay = true;
        hitObjectTransform = null;
        if(hitObjectRB) hitObjectRB.useGravity = true;
        hitObjectRB = null;
        state = State.Unoccupied;
    }
}
