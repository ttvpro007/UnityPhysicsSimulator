using UnityEngine;
using System.Collections;
using Obvious.Soap.Example;

public class Grenade : Projectile, IExplosive
{
    [Tooltip("Prefab for the explosion visual effect.")]
    [SerializeField] private GameObject explosionEffect;

    [Tooltip("Damage dealt by the explosion.")]
    [SerializeField] private float explosionDamage = 3f;

    [Tooltip("Radius of the explosion effect.")]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip("Delay before the grenade explodes.")]
    [SerializeField] private float explosionDelay = 3f;

    [Tooltip("Percentage reduction for nearby grenades.")]
    [SerializeField] private float explosionReductionPercentage = 0.05f;

    private float countdown;
    private bool hasExploded = false;

    // Properties from IExplosive
    public GameObject ExplosionEffect => explosionEffect;
    public float ExplosionDamage => explosionDamage;
    public float ExplosionRadius => explosionRadius;
    public bool HasExploded => hasExploded;

    protected override void Start()
    {
        base.Start();

        countdown = explosionDelay;

        // Start the coroutine to handle the timed explosion
        StartCoroutine(StartExplosionCountdown());
    }

    private IEnumerator StartExplosionCountdown()
    {
        // Countdown loop before explosion
        while (countdown > 0f && !hasExploded)
        {
            countdown -= Time.deltaTime; // Decrement the countdown manually
            yield return null; // Wait until the next frame
        }

        if (!hasExploded)
        {
            Explode();  // Trigger explosion
            hasExploded = true;
        }
    }

    public void Explode()
    {
        // Instantiate explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply explosion damage to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage((int)explosionDamage);
            }

            // Reduce countdown of nearby grenades
            if (nearbyObject.TryGetComponent<Grenade>(out var grenade))
            {
                grenade.ReduceCountdown(explosionReductionPercentage);
            }
        }

        // Destroy the grenade object
        Destroy();
    }

    public void ReduceCountdown(float percentage)
    {
        countdown -= explosionDelay * percentage;
    }
}