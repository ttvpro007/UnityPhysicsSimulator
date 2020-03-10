using UnityEngine;
using PhysicsSimulation.Helper;
using Core;
using AI;

namespace Weapons
{
    public class NormalGun : MonoBehaviour
    {
        [SerializeField] private float range = 0;
        [SerializeField] private float distanceToAccelerationMult = 0;
        [SerializeField] private float objectMaxSpeed = 0;
        [SerializeField] private float damage = 0;
        [SerializeField] private LayerMask hitLayer = 0;
        [SerializeField] private ParticleSystem muzzleFlashFX = null;
        [SerializeField] private ParticleSystem impactFX = null;
        private RaycastHitInfo hitInfo = null;
        private DamageDealer damageDealer = null;

        private void Start()
        {
            if (!hitInfo) hitInfo = GetComponentInParent<RaycastHitInfo>();
            damageDealer = GetComponentInParent<DamageDealer>();
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

            RaycastHit hit = hitInfo.GetHit(range, hitLayer);
            Vector3 shootDirection = hitInfo.Direction;

            if (hit.rigidbody)
            {
                float hitDistance = hit.distance;
                //float hitDistance = Vector3.Distance(transform.position, hit.point);
                float acceleration = hitDistance * distanceToAccelerationMult;
                hit.rigidbody.velocity += Mathf.Abs(range - hitDistance) * shootDirection;
                hit.rigidbody.velocity = Vector3.ClampMagnitude(hit.rigidbody.velocity, objectMaxSpeed);
                Health targetHealth = hit.transform.GetComponent<Health>();

                if (targetHealth && damageDealer)
                {
                    DamageDealer.DealDamage(targetHealth, damage);
                }

                //AIMovement aiMovement = hit.transform.GetComponent<AIMovement>();
                //if (aiMovement)
                //{
                //    aiMovement.KnockBack(shootDirection, true);
                //}
            }
        }
    }
}