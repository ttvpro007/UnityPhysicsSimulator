using Obvious.Soap.Example;
using System.Collections;
using UnityEngine;

public class Molotov : Projectile, IExplosive
{
    [Tooltip("Prefab for the explosion visual effect.")]
    [SerializeField] private GameObject explosionEffect;

    [Tooltip("Damage dealt by the explosion.")]
    [SerializeField] private float explosionDamage = 3f;

    [Tooltip("Radius of the explosion effect.")]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip("Prefab for the burn visual effect.")]
    [SerializeField] private GameObject trailEffect;

    [Tooltip("Prefab for the burn visual effect.")]
    [SerializeField] private GameObject burnEffect;

    [Tooltip("Damage dealt over time by burning.")]
    [SerializeField] private float burnDamage = 3f;

    [Tooltip("Duration of the burn effect.")]
    [SerializeField] private float burnDuration = 5f;

    [Tooltip("Interval for applying burn damage.")]
    [SerializeField] private float burnInterval = 1f;

    private float burningTimeElapsed = 0f;
    private float nextBurnDamageTime = 0f;
    private bool hasExploded = false;
    private GameObject instantiatedTrailEffect;
    private GameObject instantiatedBurnEffect;

    // Properties from IExplosive
    public GameObject ExplosionEffect => explosionEffect;
    public float ExplosionDamage => explosionDamage;
    public float ExplosionRadius => explosionRadius;
    public bool HasExploded => hasExploded;

    protected override void Start()
    {
        base.Start();

        if (trailEffect != null)
        {
            instantiatedTrailEffect = Instantiate(trailEffect, transform.position, Quaternion.identity, transform);
        }
    }

    public void Explode()
    {
        // Instantiate explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply explosion damage to nearby objects and initiate burn effect
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage((int)explosionDamage);
                StartCoroutine(ApplyBurnWhileInRadius(nearbyObject.gameObject));
            }
        }

        // Disable the molotov model
        SetModelActive(false);
    }

    private IEnumerator ApplyBurnWhileInRadius(GameObject target)
    {
        // Instantiate the burn visual effect
        if (burnEffect != null)
        {
            instantiatedBurnEffect = Instantiate(burnEffect, hitPointPosition, Quaternion.identity);
        }

        while (burningTimeElapsed < burnDuration)
        {
            if (nextBurnDamageTime >= burnInterval)
            {
                // Apply burn damage if target is within explosion radius
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

        // Destroy burn effect after burn duration ends
        if (instantiatedBurnEffect != null)
        {
            print("Destroy");
            Destroy(instantiatedBurnEffect);
        }

        // Destroy game object
        Destroy();
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

        if (!hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }
}
