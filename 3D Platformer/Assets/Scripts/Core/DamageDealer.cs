using UnityEngine;

namespace Core
{
    public class DamageDealer : MonoBehaviour
    {
        public static void DealDamage(Health target, float damage)
        {
            target.TakeDamage(damage);
        }
    }
}