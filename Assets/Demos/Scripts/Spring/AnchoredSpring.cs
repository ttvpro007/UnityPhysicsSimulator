//using Sirenix.OdinInspector;
//using UnityEngine;

//public class AnchoredSpring : MonoBehaviour
//{
//    public enum AnchorMode { None, StickyHit, Transform, WorldPoint }

//    [Header("Detection (for None/StickyHit)")]
//    public LayerMask surface = ~0;
//    public float rayExtraLength = 0.2f;               // extra reach beyond rest length

//    [Header("Spring")]
//    [Min(0f)] public float trueLength = 1.0f;         // L0
//    [Range(0f, 10000f), LabelText("Spring Constant (k)")] public float springConstant = 2000f; // N/m
//    [Range(0f, 50f), LabelText("Damping (c)")] public float dampingModifier = 50f;            // N·s/m
//    [Tooltip("Ignore pulling. Useful for suspension-like behavior.")] public bool onlyCompression = false;

//    [Header("Anchoring")]
//    public AnchorMode anchorMode = AnchorMode.None;
//    [ShowIf("@anchorMode == AnchorMode.Transform")] public Transform anchorTransform;
//    [ShowIf("@anchorMode == AnchorMode.Transform")] public Vector3 anchorLocalOffset;
//    [ShowIf("@anchorMode == AnchorMode.WorldPoint")] public Vector3 worldAnchorPoint;  // set in inspector / code
//    [ShowIf("@anchorMode == AnchorMode.StickyHit")] public float loseStickyIfFartherThan = 0.5f; // meters beyond cast

//    [Header("Apply")]
//    public bool restrained = false;
//    [Tooltip("Apply forces at contact points (gives correct torques). If false, uses center of mass.")]
//    public bool applyAtPositions = true;

//    [Header("Debug (read-only)")]
//    [ReadOnly] public float currentLength;            // |A - B|
//    [ReadOnly] public float displacement;             // x = L0 - L (compress>0, stretch<0)
//    [ReadOnly] public float compressionVelocity;      // xDot (relative along spring line)
//    [ReadOnly] public float lastScalar;               // kx - c xDot
//    [ReadOnly] public Vector3 lastAnchorWorld;        // resolved anchor for this step
//    [ReadOnly] public float maxCast;

//    public Rigidbody rb;

//    Rigidbody _anchorRb;
//    bool _hasSticky;
//    Vector3 _stickyPoint;

//    void Awake()
//    {
//        if (!rb) rb = GetComponent<Rigidbody>();
//    }

//    void Start()
//    {
//        WarnIfTooStiff();
//        CacheAnchorRb();
//    }

//    void OnValidate()
//    {
//        if (!rb) rb = GetComponent<Rigidbody>();
//        CacheAnchorRb();
//        WarnIfTooStiff();
//    }

//    void FixedUpdate()
//    {
//        if (!rb) return;

//        rb.isKinematic = restrained;
//        if (restrained) return;

//        // 1) Resolve anchor point for this step
//        Vector3 attach = transform.position; // attach point (could be an offset on your body)
//        Vector3 anchor = ResolveAnchor(attach, out bool haveAnchor);

//        if (!haveAnchor)
//            return; // in 'None' mode with no hit: no spring force at all

//        lastAnchorWorld = anchor;

//        // 2) Spring geometry along the line between points
//        Vector3 dir = attach - anchor;
//        float L = dir.magnitude;
//        if (L < 1e-6f) return;
//        Vector3 n = dir / L; // from anchor -> attach

//        currentLength = L;
//        displacement = trueLength - L; // + compress, - stretch

//        if (onlyCompression && displacement <= 0f) return;

//        // 3) Relative velocity along spring line
//        Vector3 vAttach = rb.GetPointVelocity(attach);
//        Vector3 vAnchor = Vector3.zero;
//        if (_anchorRb) vAnchor = _anchorRb.GetPointVelocity(anchor);

//        compressionVelocity = Vector3.Dot(vAttach - vAnchor, n);

//        // 4) Spring-damper force
//        float f = springConstant * displacement - dampingModifier * compressionVelocity;
//        lastScalar = f;

//        Vector3 force = f * n;

//        if (applyAtPositions)
//        {
//            rb.AddForceAtPosition(force, attach, ForceMode.Force);
//            if (_anchorRb) _anchorRb.AddForceAtPosition(-force, anchor, ForceMode.Force);
//        }
//        else
//        {
//            rb.AddForce(force, ForceMode.Force);
//            if (_anchorRb) _anchorRb.AddForce(-force, ForceMode.Force);
//        }
//    }

//    // ----------------- Anchor resolution -----------------
//    Vector3 ResolveAnchor(Vector3 attach, out bool haveAnchor)
//    {
//        haveAnchor = false;

//        switch (anchorMode)
//        {
//            case AnchorMode.Transform:
//                {
//                    if (!anchorTransform) return default;
//                    haveAnchor = true;
//                    return anchorTransform.TransformPoint(anchorLocalOffset);
//                }

//            case AnchorMode.WorldPoint:
//                {
//                    haveAnchor = true;
//                    return worldAnchorPoint;
//                }

//            case AnchorMode.StickyHit:
//            case AnchorMode.None:
//                {
//                    // Use a down-ray along -transform.up to find ground contact
//                    maxCast = trueLength + rayExtraLength;
//                    Ray ray = new Ray(transform.position, -transform.up);
//                    if (Physics.Raycast(ray, out var hit, maxCast, surface, QueryTriggerInteraction.Ignore))
//                    {
//                        _stickyPoint = hit.point;
//                        _hasSticky = true;
//                        haveAnchor = true;
//                        return _stickyPoint;
//                    }

//                    if (anchorMode == AnchorMode.StickyHit && _hasSticky)
//                    {
//                        // Keep last sticky point unless we've drifted too far away
//                        float beyond = (attach - _stickyPoint).magnitude - maxCast;
//                        if (beyond <= loseStickyIfFartherThan)
//                        {
//                            haveAnchor = true;
//                            return _stickyPoint;
//                        }
//                        else
//                        {
//                            _hasSticky = false; // drop anchor
//                        }
//                    }

//                    // 'None' mode: no contact -> no anchor -> no force
//                    return default;
//                }
//        }

//        return default;
//    }

//    void CacheAnchorRb()
//    {
//        _anchorRb = null;
//        if (anchorTransform) anchorTransform.TryGetComponent(out _anchorRb);
//    }

//    // --------------- Stability heuristic ---------------
//    void WarnIfTooStiff()
//    {
//        float mEff = rb ? rb.mass : 1f;
//        if (_anchorRb) mEff = 1f / (1f / Mathf.Max(1e-6f, rb.mass) + 1f / Mathf.Max(1e-6f, _anchorRb.mass)); // reduced mass

//        float kMax = mEff * Mathf.Pow(2f * Mathf.PI / (8f * Time.fixedDeltaTime), 2f);
//        if (springConstant > kMax)
//        {
//            Debug.LogWarning(
//                $"[AnchoredSpring] k={springConstant:F0} > kMax≈{kMax:F0} (m_eff={mEff:F2}, dt={Time.fixedDeltaTime}). " +
//                $"Consider lowering k, raising mass, or increasing solver substeps.");
//        }
//    }

////#if UNITY_EDITOR
////    void OnDrawGizmosSelected()
////    {
////        Gizmos.color = Color.cyan;
////        Vector3 attach = transform.position;
////        Vector3 anchor = lastAnchorWorld;
////        if (anchorMode == AnchorMode.WorldPoint) anchor = worldAnchorPoint;
////        else if (anchorMode == AnchorMode.Transform && anchorTransform) anchor = anchorTransform.TransformPoint(anchorLocalOffset);

////        if (anchor != default)
////        {
////            Gizmos.DrawLine(attach, anchor);
////            Gizmos.DrawSphere(anchor, 0.03f);
////        }
////    }
////#endif
//}
