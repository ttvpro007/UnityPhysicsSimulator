using Obvious.Soap.Example;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Projectile : MonoBehaviour
{
    /// <summary>
    /// The amount of damage this projectile deals upon impact.
    /// </summary>
    [Tooltip("The amount of damage this projectile deals upon impact.")]
    [SerializeField] protected int onHitDamage;
    [SerializeField] protected Vector2 modifierPercentage;

    [SerializeField] private GameObject hitPointPrefab;

    [SerializeField] private GameObject onHitEffect;      // Prefab for the explosion visual effect

    [SerializeField] private float critChance;

    protected Rigidbody rBody;
    protected Collider col;
    protected Transform modelTransform;

    public void SetHitPointPosition(Vector3 hitPointPosition)
    {
        this.hitPointPosition = hitPointPosition;
    }

    protected Vector3 hitPointPosition;
    protected GameObject hitPointInstance;

    protected virtual void Awake()
    {
        if (!TryGetComponent(out rBody))
        {
            rBody = gameObject.AddComponent<Rigidbody>();
        }

        col = gameObject.GetComponent<Collider>();

        modelTransform = transform.Find("Model");
    }

    protected virtual void Start()
    {
        hitPointInstance = Instantiate(hitPointPrefab, hitPointPosition, Quaternion.identity);
    }

    protected virtual void Destroy()
    {
        Destroy(hitPointInstance);
        Destroy(gameObject);
    }

    protected float GetCalculatedDamage(float damage)
    {
        damage *= (1f + Random.Range(modifierPercentage.x, modifierPercentage.y) / 100f);

        if (Random.Range(0f, 1f) <= critChance)
        {
            damage *= 2f;
        }

        return damage;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Instantiate on hit effect at grenade's position
        if (onHitEffect != null)
        {
            Instantiate(onHitEffect, transform.position, transform.rotation);
        }

        // Apply damage to enemy health scripts
        if (collision.gameObject.TryGetComponent<Health>(out var health))
        {
            DealDamage(health, onHitDamage);
        }
    }

    protected virtual void DealDamage(Health health, float damage)
    {
        health.TakeDamage((int)GetCalculatedDamage(damage));
    }
}