using Obvious.Soap.Example;
using UnityEngine;

public class Grenade : Projectile
{
    [SerializeField] private GameObject explosionEffect;      // Prefab for the explosion visual effect
    [SerializeField] private float explosionDamage = 3f;       // Delay before the grenade explodes
    [SerializeField] private float explosionDelay = 3f;       // Delay before the grenade explodes
    [SerializeField] private float explosionRadius = 3f;      // Radius of the explosion effect
    [SerializeField] private float explosionReductionPercentage = .05f;      // Radius of the explosion effect

    private float countdown;
    private bool hasExploded = false;

    protected override void Start()
    {
        base.Start();

        // Set the countdown timer
        countdown = explosionDelay;
    }

    private void Update()
    {
        // Countdown before explosion
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }

    private void Explode()
    {
        // Instantiate explosion effect at grenade's position
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // Find all nearby objects within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // Apply damage to enemy health scripts
            if (nearbyObject.TryGetComponent<Health>(out var health))
            {
                DealDamage(health, explosionDamage);
            }

            // Apply time reduction to nearby grenades
            if (nearbyObject.TryGetComponent<Grenade>(out var grenade))
            {
                grenade.ReduceCountdown(explosionReductionPercentage);
            }
        }

        // Destroy the grenade after its lifetime
        Destroy();
    }

    public void ReduceCountdown(float percentage)
    {
        countdown -= explosionDelay * percentage;
    }
}
