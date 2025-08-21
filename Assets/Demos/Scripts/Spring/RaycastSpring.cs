using UnityEngine;

/// <summary>
/// Raycast-based 1-D spring/damper (suspension-style) that applies force along the
/// object's local +Y axis toward a surface detected below.
/// </summary>
/// <remarks>
/// Physics model (SI units):
/// • Hooke + viscous damping:  F = k·x − c·ẋ  
/// • Compression:             x = restLength − hitDistance  (clamped to ≥ 0 when <see cref="onlyPush"/> is true)  
/// • Damping from ratio:      c = 2·√(k·m)·ζ  where m = Rigidbody.mass and ζ = <see cref="dampingRatio"/>
/// 
/// Implementation notes:
/// • Force is applied along <c>transform.TransformDirection(Vector3.up)</c>.  
/// • Evaluated in <c>FixedUpdate</c> with Δt = <c>Time.fixedDeltaTime</c>.  
/// • Ray length = restLength + rayExtraLength; if no hit, no spring force is applied.  
/// • Unity’s built-in gravity is used (no extra gravity force here).  
/// • <see cref="SetRestrained(bool)"/> zeros velocities and disables spring response.
/// </remarks>
[RequireComponent(typeof(Rigidbody))]
public class RaycastSpring : MonoBehaviour
{
    [Header("Spring Setup")]
    [SerializeField] private LayerMask surface = ~0;
    [SerializeField, Min(0f)] private float restLength = 1.0f;     // meters
    [SerializeField, Min(0f)] private float springConstant = 1000f; // N/m
    [SerializeField, Range(0f, 3f)] private float dampingRatio = 1.0f; // ζ (0=undamped, 1=critical)
    [SerializeField, Min(0f)] private float rayExtraLength = 0.2f; // extra ray reach beyond rest length
    [SerializeField] private bool onlyPush = true; // true = no pulling force
    [SerializeField] private bool restrained = false;

    // Runtime/state (hidden)
    private Rigidbody rb;
    private float prevCompression = 0f;

    // Read-only observability
    public float CurrentDistance { get; private set; } = float.PositiveInfinity; // distance to surface along -up
    public float Compression { get; private set; } = 0f;                     // x = restLength - CurrentDistance (>=0 if onlyPush)
    public float CompressionRate { get; private set; } = 0f;                     // dx/dt (m/s), positive when compressing

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (restrained)
        {
            // Keep rigidbody dynamic but frozen for stability
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            prevCompression = 0f;
            Compression = 0f;
            CompressionRate = 0f;
            CurrentDistance = float.PositiveInfinity;
            return;
        }

        Vector3 axis = transform.TransformDirection(Vector3.up);   // local up as spring axis
        Vector3 rayDir = -axis;
        float maxRay = restLength + rayExtraLength;

        if (Physics.Raycast(transform.position, rayDir, out RaycastHit hit, maxRay, surface, QueryTriggerInteraction.Ignore))
        {
            CurrentDistance = hit.distance;

            // Compression (how much shorter than rest length)
            float compression = restLength - CurrentDistance;
            if (onlyPush) compression = Mathf.Max(0f, compression);

            // Compression rate using finite difference over fixed timestep
            float dt = Time.fixedDeltaTime;
            float rate = (compression - prevCompression) / Mathf.Max(1e-6f, dt);

            // Damping coefficient from ratio: c = 2 * sqrt(k*m) * ζ
            float c = 2f * Mathf.Sqrt(Mathf.Max(1e-6f, springConstant * rb.mass)) * dampingRatio;

            // Hooke + damping (note the minus on damping)
            float forceMag = springConstant * compression - c * rate;

            // If push-only, keep force non-negative along +axis
            if (onlyPush) forceMag = Mathf.Max(0f, forceMag);

            // Apply along spring axis
            rb.AddForce(axis * forceMag, ForceMode.Force);

            // Update state
            Compression = compression;
            CompressionRate = rate;
            prevCompression = compression;

#if UNITY_EDITOR
            Debug.DrawRay(transform.position, rayDir * hit.distance, Color.red);
#endif
        }
        else
        {
            // No contact within range → no spring force
            CurrentDistance = float.PositiveInfinity;
            CompressionRate = 0f;
            Compression = 0f;
            prevCompression = 0f;
        }
    }

    public void SetRestrained(bool value) => restrained = value;

    private void OnValidate()
    {
        restLength = Mathf.Max(0f, restLength);
        springConstant = Mathf.Max(0f, springConstant);
        rayExtraLength = Mathf.Max(0f, rayExtraLength);
        dampingRatio = Mathf.Clamp(dampingRatio, 0f, 3f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // visualize rest length ray from this transform along -up
        Gizmos.color = Color.cyan;
        Vector3 axis = Application.isPlaying ? transform.TransformDirection(Vector3.up)
                                             : transform.up;
        Gizmos.DrawLine(transform.position, transform.position - axis * restLength);
    }
#endif
}
