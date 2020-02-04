using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float range = 0;
    [SerializeField] private float impactForce = 0;
    [SerializeField] private ParticleSystem muzzleFlashFX = null;
    [SerializeField] private ParticleSystem impactFX = null;
    private RaycastHitInfo hitInfo = null;

    private void Start()
    {
        if (!hitInfo) hitInfo = GetComponentInParent<RaycastHitInfo>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (muzzleFlashFX) muzzleFlashFX.Play();

        RaycastHit hit = hitInfo.GetHit(range);
        Vector3 shootDirection = hitInfo.Direction;

        if (hit.rigidbody)
        {
            hit.rigidbody.AddForce(shootDirection * impactForce);
        }
    }
}