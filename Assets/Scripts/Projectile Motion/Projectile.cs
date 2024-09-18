using UnityEngine;

public class Projectile : MonoBehaviour
{
    // This script no longer handles launching or physics parameters.
    // The spawner/thrower will manage the launching mechanics.

    // Optional: Still keep collision logic here
    void OnCollisionEnter(Collision collision)
    {
        // Handle what happens when the projectile hits something
        Destroy(gameObject); // Destroy the projectile on impact
    }
}
