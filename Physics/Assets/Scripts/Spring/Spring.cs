using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] LayerMask surface;
    [SerializeField] float trueLength;
    [Range(0f, 4500f)]
    [SerializeField] float springConstant;
    [Range(1f, 50f)]
    [SerializeField] float dampingModifier;
    [SerializeField] bool restrained;
    float displacement;
    float beginOfFrameDisplacement = 0;
    public float deltaDisplacement { get { return displacement - beginOfFrameDisplacement; } }
    public float currentLength;
    Vector3 gForce;
    Vector3 springForce;
    Vector3 dampingForce;
    Vector3 gravity;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravity = Physics.gravity;
        CalculateForces();
    }

    private void FixedUpdate()
    {
        CalculateForces();
    }

    private void CalculateForces()
    {
        // Activate if not restrained
        if (restrained)
        {
            rb.isKinematic = true;
            return;
        }
        else if (rb.isKinematic && !restrained)
        {
            rb.isKinematic = false;
        }

        // Setup variable to calculate deltaX
        beginOfFrameDisplacement = displacement;
        currentLength = GetDistanceToSurface();
        displacement = trueLength - currentLength;

        // Forces
        gForce = rb.mass * gravity;
        dampingForce = Vector3.up * (dampingModifier * deltaDisplacement / Time.deltaTime);
        springForce = transform.TransformVector(Vector3.up) * (springConstant * displacement + dampingForce.y);

        // Apply spring force
        rb.AddForce(springForce);
    }

    private float GetDistanceToSurface()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformVector(Vector3.down), out hit, Mathf.Infinity, surface))
        {
            Debug.DrawRay(transform.position, transform.TransformVector(Vector3.down) * hit.distance, Color.red);
            return hit.distance;
        }

        return -1;
    }
}