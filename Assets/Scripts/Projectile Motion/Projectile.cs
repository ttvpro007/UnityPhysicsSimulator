using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    /// <summary>
    /// Handles the logic for when the projectile hits something.
    /// Currently destroys the projectile but can be extended for additional behavior.
    /// </summary>
    /// <param name="collision">Information about the collision.</param>
    private void HandleCollision(Collision collision)
    {
        // Optional: Add logic here for what happens when the projectile hits something (e.g., dealing damage, spawning effects)
        Destroy(gameObject); // Destroy the projectile on impact
    }
}
