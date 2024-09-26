using Obvious.Soap.Example;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /// <summary>
    /// The amount of damage this projectile deals upon impact.
    /// </summary>
    [Tooltip("The amount of damage this projectile deals upon impact.")]
    [SerializeField] private int damageAmount;

    [SerializeField] private GameObject hitPointPrefab;

    public void SetHitPointPosition(Vector3 hitPointPosition)
    {
        this.hitPointPosition = hitPointPosition;
    }

    private Vector3 hitPointPosition;
    private GameObject hitPointInstance;

    private void Start()
    {
        hitPointInstance = Instantiate(hitPointPrefab, hitPointPosition, Quaternion.identity);
    }

    /// <summary>
    /// Unity callback method invoked when this object collides with another object.
    /// Triggers <see cref="HandleCollision"/> to manage the collision behavior.
    /// </summary>
    /// <param name="collision">Contains information about the collision event.</param>
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    /// <summary>
    /// Handles the logic for when the projectile hits an object.
    /// It checks if the collided object has a <see cref="Health"/> component and applies damage if applicable.
    /// The projectile is destroyed upon collision.
    /// </summary>
    /// <param name="collision">Information about the collision, including the object hit.</param>
    private void HandleCollision(Collision collision)
    {
        // Check if the object hit has a Health component
        if (collision.gameObject.TryGetComponent<Health>(out var health))
        {
            // Apply damage if the Health component is found
            DealDamage(health);
        }

        Destroy(hitPointInstance);
        
        // Destroy the projectile after collision
        Destroy(gameObject);
    }

    /// <summary>
    /// Inflicts damage on the target by calling its <see cref="Health.TakeDamage"/> method.
    /// </summary>
    /// <param name="health">The health component of the target receiving damage.</param>
    private void DealDamage(Health health)
    {
        // Reduce the health of the object by the specified damage amount
        health.TakeDamage(damageAmount);
    }
}