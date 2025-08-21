using UnityEngine;

public class Spring : MonoBehaviour
{
    [Header("Detection")]
    public LayerMask surface = ~0;
    public float rayExtraLength = 0.2f;

    [Header("Spring")]
    public float trueLength = 1.0f;                             // L0
    [Range(0f, 4500f)] public float springConstant = 2000f;     // k (N/m)
    [Range(0f, 50f)] public float dampingModifier = 50f;        // c (N·s/m)
    public bool restrained = false;

    [Header("When no hit")]
    public bool clampWhenNoHitToPull = true;                    // keep a small negative x to “pull” back

    [Header("Debug (read-only)")]
    public float displacement;                                  // x
    public float beginOfFrameDisplacement;
    public float deltaDisplacement;                             // x_t - x_{t-Δt} (not used in force anymore)
    public float currentLength;                                 // L (distance to surface along -transform.up)
    public float compressionVelocity;                           // xDot used this step
    public float lastScalar;
    public float maxCast;                                       // max ray length

    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate() => ApplySpring();

    private void ApplySpring()
    {
        if (!rb) return;

        rb.isKinematic = restrained;
        if (restrained) return;

        Ray ray = new(transform.position, -transform.up);

        // If no hit, keep a small negative x so it “pulls” back instead of popping upward
        maxCast = trueLength + rayExtraLength;
        if (Physics.Raycast(ray, out var hit, maxCast, surface, QueryTriggerInteraction.Ignore))
            currentLength = hit.distance;
        else
            currentLength = clampWhenNoHitToPull ? maxCast : trueLength;

        // Compression (can be +/-). Positive => push; negative => pull.
        beginOfFrameDisplacement = displacement;
        displacement = trueLength - currentLength;
        deltaDisplacement = displacement - beginOfFrameDisplacement;

        // >>> KEY FIX: use axis velocity for xDot, not discrete dx/dt <<<
        compressionVelocity = Vector3.Dot(rb.linearVelocity, transform.up);
        lastScalar = springConstant * displacement - dampingModifier * compressionVelocity; // F = kx - c xDot

        rb.AddForce(lastScalar * transform.up, ForceMode.Force);

        //// Debug
        //Debug.DrawRay(transform.position, -transform.up * Mathf.Min(currentLength, maxCast), Color.red);
        //Debug.DrawRay(transform.position, transform.up * Mathf.Clamp(displacement, -trueLength, trueLength), displacement >= 0 ? Color.yellow : Color.cyan);
    }
}
