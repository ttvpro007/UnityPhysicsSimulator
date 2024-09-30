using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    /// <summary>
    /// The amount of damage this projectile deals upon impact.
    /// </summary>
    [Tooltip("The amount of damage this projectile deals upon impact.")]
    [SerializeField] protected int damageAmount;
    [SerializeField] protected Vector2 modifierPercentage;

    [SerializeField] private GameObject hitPointPrefab;

    public void SetHitPointPosition(Vector3 hitPointPosition)
    {
        this.hitPointPosition = hitPointPosition;
    }

    private Vector3 hitPointPosition;
    private GameObject hitPointInstance;

    protected virtual void Start()
    {
        hitPointInstance = Instantiate(hitPointPrefab, hitPointPosition, Quaternion.identity);
    }

    protected virtual void Destroy()
    {
        Destroy(hitPointInstance);
        Destroy(gameObject);
    }

    protected float GetCalculatedDamageAmount()
    {
        return damageAmount * (1f + Random.Range(modifierPercentage.x, modifierPercentage.y) / 100f);
    }
}