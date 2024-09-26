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
    public float angle;
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
        angle = GetAngle(GetRaycast());
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
        currentLength = GetDistanceToSurface(GetRaycast());
        displacement = trueLength - currentLength;

        // Forces
        gForce = rb.mass * gravity;
        dampingForce = Vector3.up * (dampingModifier * deltaDisplacement / Time.deltaTime);
        springForce = transform.TransformVector(Vector3.up) * (springConstant * displacement + dampingForce.y);

        // Apply spring force
        rb.AddForce(springForce);
    }

    private Ray GetRaycast()
    {
        Ray ray = new Ray(transform.position, transform.TransformVector(Vector3.down));
        return ray;
    }

    private float GetDistanceToSurface(Ray ray)
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, surface);

        if (hasHit)
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            return hit.distance;
        }

        return -1;
    }

    private float GetAngle(Ray ray)
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, Mathf.Infinity, surface);

        if (hasHit)
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            return 90 - Vector3.Angle(-1 * ray.direction, hit.normal);
        }

        return Mathf.NegativeInfinity;
    }
}