using UnityEngine;

public class GrenadeProperty : MonoBehaviour
{
    [SerializeField] float lifeTime = 10f;
    private Rigidbody rb;
    private bool isThrown = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing on Grenade.");
        }

        // Destroy the grenade after its lifetime
        Destroy(gameObject, lifeTime);
    }

    // Method to apply force and visualize the direction of the throw
    public void ApplyForce(Vector3 initialForce)
    {
        if (rb != null && !isThrown)
        {
            // Apply the force
            rb.AddForce(initialForce, ForceMode.VelocityChange);
            isThrown = true;

            // Draw a debug line to show the grenade's direction, length based on velocity magnitude
            float forceMagnitude = initialForce.magnitude;  // Get the magnitude of the force (velocity)
            Debug.DrawLine(transform.position, transform.position + initialForce.normalized * forceMagnitude, Color.red, 2f);

            // Log the direction and magnitude for debugging
            Debug.Log("Grenade direction: " + initialForce.normalized + ", Velocity magnitude: " + forceMagnitude);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    // Handle grenade impact, for example, destroy the grenade or trigger explosion effects
    //    Destroy(gameObject);
    //}
}
