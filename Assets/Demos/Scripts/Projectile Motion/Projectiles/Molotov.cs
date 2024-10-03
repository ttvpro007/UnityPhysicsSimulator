using Obvious.Soap.Example;
using System.Collections;
using UnityEngine;

public class Molotov : Projectile
{
    [SerializeField] private GameObject trailEffect;          // Prefab for the trail visual effect
    [SerializeField] private GameObject explosionEffect;      // Prefab for the explosion visual effect
    [SerializeField] private float explosionDamage = 3f;      // Damage dealt by the explosion
    [SerializeField] private float explosionRadius = 3f;      // Radius of the explosion effect
    [SerializeField] private GameObject burnEffect;           // Prefab for the burn visual effect
    [SerializeField] private float burnDamage = 3f;           // Damage dealt over time by burning
    [SerializeField] private float burnDuration = 5f;         // Duration of the burn effect
    [SerializeField] private float burnInterval = 1f;         // Interval for applying burn damage

    private GameObject instantiatedTrailEffect;

    protected override void Start()
    {
        base.Start();

        // Instantiate and attach the trail effect to the molotov
        if (trailEffect != null)
        {
            instantiatedTrailEffect = Instantiate(trailEffect, transform.position, Quaternion.identity, transform);
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Stop physics and disable the trail effect
        rBody.isKinematic = true;
        if (col) col.isTrigger = true;

        if (instantiatedTrailEffect != null)
        {
            Destroy(instantiatedTrailEffect);
        }

        // Create the explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Deal immediate explosion damage to objects in the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.TryGetComponent<Health>(out var health))
            {
                // Apply explosion damage
                health.TakeDamage((int)burnDamage);

                // Start burn effect and damage over time while within the radius
                StartCoroutine(ApplyBurnWhileInRadius(nearbyObject.gameObject));
            }
        }

        // Deactivate the molotov model
        if (modelTransform) modelTransform.gameObject.SetActive(false);
    }

    private float burningTimeElapsed = 0f;
    private float nextBurnDamageTime = 0f;
    private IEnumerator ApplyBurnWhileInRadius(GameObject target)
    {
        GameObject instantiatedBurnEffect = null;

        if (burnEffect != null)
        {
            // Create the burn visual effect at the hit point position
            instantiatedBurnEffect = Instantiate(burnEffect, hitPointPosition, Quaternion.identity);
        }

        while (burningTimeElapsed < burnDuration)
        {
            if (nextBurnDamageTime >= burnInterval)
            {
                // Check if the target is still within the burn radius
                if (Vector3.Distance(hitPointPosition, target.transform.position) <= explosionRadius)
                {
                    if (target.TryGetComponent<Health>(out var health))
                    {
                        health.TakeDamage((int)burnDamage);
                    }
                }

                nextBurnDamageTime = 0f;
            }

            burningTimeElapsed += Time.deltaTime;
            nextBurnDamageTime += Time.deltaTime;
            yield return null;
        }

        // Clean up burn effect after the burn ends or if the target leaves the radius
        if (instantiatedBurnEffect != null)
        {
            print("Destroy");
            Destroy(instantiatedBurnEffect);
        }

        // Destroy the molotov object
        Destroy();
    }
}
