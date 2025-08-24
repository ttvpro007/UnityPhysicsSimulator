using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(10000)]
public class RuntimeTransform : MonoBehaviour, IDraggableHandler
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────────────────────────────────────
    const float kParallelEpsilon = 1e-6f;
    const int kNoPointerId = -999;

    // ─────────────────────────────────────────────────────────────────────────────
    // Config
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("Setup")]
    public Camera sceneCamera;

    [Header("Behavior")]
    [Tooltip("World- or local-aligned axes")]
    public bool useLocalSpace = false;

    public bool autoSize = true;
    [Range(0.01f, 1f)] public float sizeFactor = 0.15f;
    [Tooltip("Global position snapping step (overridden per-axis if set)")]
    public float snapStep = 0f;

    // ─────────────────────────────────────────────────────────────────────────────
    // State (read-only intent)
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("State (read-only)")]
    public Transform target;
    public RuntimeTransformAxis hovered;
    public RuntimeTransformAxis active;
    public bool isDragging;

    // Private runtime
    RuntimeTransformAxis[] _handles;
    Vector3 _dragAxisWorld;
    Vector3 _targetStartPos;
    Vector3 _dragPlaneOrigin;
    Plane _dragPlane;
    int _activePointerId = kNoPointerId;

    // ─────────────────────────────────────────────────────────────────────────────
    // Axis Constraints (types + config)
    // ─────────────────────────────────────────────────────────────────────────────
    [Flags] public enum AxisMask { None = 0, X = 1, Y = 2, Z = 4, All = X | Y | Z }

    [Serializable]
    public struct AxisLimit
    {
        [Tooltip("If true, clamp delta between [min,max] relative to drag start.")]
        public bool useLimit;
        [Tooltip("Negative = backward along the axis from the drag start.")]
        public float min;
        [Tooltip("Positive = forward along the axis from the drag start.")]
        public float max;
        [Min(0f), Tooltip("Per-axis snap. 0 = use global snapStep.")]
        public float snap;
    }

    [Header("Axis Constraints")]
    public AxisMask allowedAxes = AxisMask.All;
    public AxisLimit xLimit;
    public AxisLimit yLimit;
    public AxisLimit zLimit;

    [Tooltip("Optional world-space box the target's position must remain inside. Not serialized when null.")]
    public Bounds? worldBounds = null;

    [Header("Constraint Modifiers")]
    public bool holdShiftDisablesSnap = true;
    public bool holdCtrlBypassesLocks = true;

    // ─────────────────────────────────────────────────────────────────────────────
    // Unity Messages
    // ─────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!sceneCamera) sceneCamera = Camera.main;
        _handles = GetComponentsInChildren<RuntimeTransformAxis>(includeInactive: true);
        SetAllStates(HandleState.Normal);
        ApplyHandleVisibilityByLocks();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!target || !sceneCamera) return;

        // Follow target
        transform.position = target.position;
        transform.rotation = useLocalSpace ? target.rotation : Quaternion.identity;

        if (autoSize)
        {
            float dist = Vector3.Distance(sceneCamera.transform.position, transform.position);
            float s = Mathf.Max(0.0001f, dist * sizeFactor);
            transform.localScale = Vector3.one * s;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Public Control API
    // ─────────────────────────────────────────────────────────────────────────────
    public void Attach(Transform newTarget)
    {
        target = newTarget;
        if (target)
        {
            transform.position = target.position;
            transform.rotation = useLocalSpace ? target.rotation : Quaternion.identity;
            gameObject.SetActive(true);
        }
        else Detach();
    }

    public void Detach()
    {
        target = null;
        isDragging = false;
        hovered = active = null;
        gameObject.SetActive(false);
    }

    // Axis-constraint API
    public void SetAxisAllowed(Axis a, bool allowed)
    {
        AxisMask bit = a switch { Axis.X => AxisMask.X, Axis.Y => AxisMask.Y, Axis.Z => AxisMask.Z, _ => AxisMask.None };
        if (allowed) allowedAxes |= bit; else allowedAxes &= ~bit;
        ApplyHandleVisibilityByLocks();
    }

    public void AllowOnly(Axis a)
    {
        allowedAxes = a switch
        {
            Axis.X => AxisMask.X,
            Axis.Y => AxisMask.Y,
            Axis.Z => AxisMask.Z,
            _ => AxisMask.None
        };
        ApplyHandleVisibilityByLocks();
    }

    public void ClearLimits()
    {
        xLimit.useLimit = yLimit.useLimit = zLimit.useLimit = false;
    }

    public void SetLimit(Axis a, float min, float max, float perAxisSnap = 0f)
    {
        var newLim = new AxisLimit { useLimit = true, min = min, max = max, snap = perAxisSnap };
        switch (a)
        {
            case Axis.X: xLimit = newLim; break;
            case Axis.Y: yLimit = newLim; break;
            case Axis.Z: zLimit = newLim; break;
            default: throw new ArgumentOutOfRangeException(nameof(a), a, null);
        }
    }

    public void SetWorldBounds(Bounds? bounds) => worldBounds = bounds;

    // ─────────────────────────────────────────────────────────────────────────────
    // Event Entry Points (called by RuntimeTransformAxis)
    // ─────────────────────────────────────────────────────────────────────────────
    public void HandlePointerEnter(ISelectable selectable, PointerEventData eventData)
    {
        if (selectable is not RuntimeTransformAxis rta) return;
        if (!target || isDragging) return;

        if (hovered != rta)
        {
            SetAllStates(HandleState.Normal);
            rta.SetState(HandleState.Hover);
            hovered = rta;
        }
    }

    public void HandlePointerExit(ISelectable selectable, PointerEventData eventData)
    {
        if (selectable is not RuntimeTransformAxis rta) return;
        if (!target || isDragging) return;

        if (hovered == rta)
        {
            rta.SetState(HandleState.Normal);
            hovered = null;
        }
    }

    public void HandlePointerDown(ISelectable selectable, PointerEventData eventData)
    {
        if (selectable is not RuntimeTransformAxis rta) return;
        if (!target) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        bool bypass = holdCtrlBypassesLocks &&
                      (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        if (!bypass && !Has(allowedAxes, rta.axis)) return;

        _activePointerId = eventData.pointerId;
    }

    public void HandlePointerUp(ISelectable selectable, PointerEventData eventData)
    {
        if (selectable is not RuntimeTransformAxis) return;
        if (eventData.pointerId != _activePointerId) return;
        _activePointerId = kNoPointerId;
    }

    public void HandleBeginDrag(IDraggable interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return;
        if (!target) return;
        if (eventData.pointerId != _activePointerId) return;

        bool bypass = holdCtrlBypassesLocks &&
                      (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        if (!bypass && !Has(allowedAxes, rta.axis)) return;

        active = rta;
        isDragging = true;
        _targetStartPos = target.position;

        // Axis in world
        _dragAxisWorld = GetWorldAxis(active.axis).normalized;

        // Plane that contains the axis and faces the camera
        Camera cam = EventCamera(eventData);
        Vector3 camFwd = cam ? cam.transform.forward : Vector3.forward;
        Vector3 n = Vector3.ProjectOnPlane(camFwd, _dragAxisWorld);
        if (n.sqrMagnitude < kParallelEpsilon)
            n = Vector3.ProjectOnPlane((cam ? cam.transform.up : Vector3.up), _dragAxisWorld);
        if (n.sqrMagnitude < kParallelEpsilon) n = Vector3.right;
        n.Normalize();

        _dragPlane = new Plane(n, _targetStartPos);

        // Starting point
        Ray r = EventRay(eventData);
        _dragPlaneOrigin = RayToPlane(r, _dragPlane, _targetStartPos);

        SetAllStates(HandleState.Normal);
        active.SetState(HandleState.Active);
    }

    public void HandleDrag(IDraggable interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return;
        if (!target || !isDragging || active != rta) return;

        Ray r = EventRay(eventData);
        Vector3 currOnPlane = RayToPlane(r, _dragPlane, _dragPlaneOrigin);
        float rawDelta = Vector3.Dot(currOnPlane - _dragPlaneOrigin, _dragAxisWorld);

        // Per-axis limits & snap
        float delta = ApplyAxisLimitsAndSnap(active.axis, rawDelta);

        // Candidate and final (respect optional world bounds)
        Vector3 candidate = _targetStartPos + _dragAxisWorld * delta;
        candidate = ClampToWorldBounds(candidate);

        target.position = candidate;
    }

    public void HandleEndDrag(IDraggable interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return;
        if (active != rta) return;

        isDragging = false;
        _activePointerId = kNoPointerId;

        if (hovered == rta) rta.SetState(HandleState.Hover);
        else rta.SetState(HandleState.Normal);

        active = null;

        // Refresh handle visibility in case locks toggled mid-drag
        ApplyHandleVisibilityByLocks();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────
    Vector3 GetWorldAxis(Axis a) => a switch
    {
        Axis.X => transform.right,
        Axis.Y => transform.up,
        Axis.Z => transform.forward,
        _ => Vector3.zero,
    };

    void SetAllStates(HandleState s)
    {
        if (_handles == null) return;
        foreach (var h in _handles)
        {
            if (!h) continue;
            if (!Has(allowedAxes, h.axis))
            {
                if (h.gameObject.activeSelf) h.gameObject.SetActive(false);
                continue;
            }
            h.SetState(s);
        }
    }

    Camera EventCamera(PointerEventData e)
    {
        if (e.pressEventCamera) return e.pressEventCamera;
        if (e.enterEventCamera) return e.enterEventCamera;
        return sceneCamera ? sceneCamera : Camera.main;
    }

    Ray EventRay(PointerEventData e)
    {
        var cam = EventCamera(e);
        var pos = e.position;
        if (cam) return cam.ScreenPointToRay(pos);
        // Fallback: default forward ray from camera-less world origin
        return new Ray(new Vector3(pos.x, pos.y, -1000f), Vector3.forward);
    }

    Vector3 RayToPlane(Ray r, Plane plane, Vector3 fallbackPoint)
    {
        if (plane.Raycast(r, out float enter))
            return r.origin + r.direction * enter;

        // Fallback: closest point on axis line if plane-ray nearly parallel
        Vector3 axisPoint = _targetStartPos;
        float s = ClosestPointParamOnAxisToRay(axisPoint, _dragAxisWorld, r);
        return axisPoint + _dragAxisWorld * s;
    }

    float ClosestPointParamOnAxisToRay(Vector3 axisPoint, Vector3 axisDir, Ray ray)
    {
        Vector3 u = axisDir.normalized;
        Vector3 v = ray.direction.normalized;
        Vector3 w0 = axisPoint - ray.origin;

        float b = Vector3.Dot(u, v);
        float d = Vector3.Dot(u, w0);
        float e = Vector3.Dot(v, w0);
        float D = 1f - b * b; // since ||u||=||v||=1

        if (Mathf.Abs(D) < kParallelEpsilon) return 0f;
        return (b * e - d) / D;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Constraint Helpers
    // ─────────────────────────────────────────────────────────────────────────────
    AxisLimit GetLimit(Axis a) => a switch
    {
        Axis.X => xLimit,
        Axis.Y => yLimit,
        Axis.Z => zLimit,
        _ => default
    };

    static bool Has(AxisMask mask, Axis a)
    {
        AxisMask bit = a switch { Axis.X => AxisMask.X, Axis.Y => AxisMask.Y, Axis.Z => AxisMask.Z, _ => AxisMask.None };
        return (mask & bit) != 0;
    }

    void ApplyHandleVisibilityByLocks()
    {
        if (_handles == null) return;
        foreach (var h in _handles)
        {
            if (!h) continue;
            bool allow = Has(allowedAxes, h.axis);
            if (h.gameObject.activeSelf != allow) h.gameObject.SetActive(allow);
        }
    }

    float ApplyAxisLimitsAndSnap(Axis axis, float rawDelta)
    {
        var lim = GetLimit(axis);

        // Limits (relative to _targetStartPos along active axis)
        if (lim.useLimit)
            rawDelta = Mathf.Clamp(rawDelta, lim.min, lim.max);

        // Choose snap (per-axis overrides global)
        float snap = lim.snap > 0f ? lim.snap : snapStep;

        // Hold Shift to temporarily disable snapping
        if (holdShiftDisablesSnap && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            snap = 0f;

        if (snap > 0f)
            rawDelta = Mathf.Round(rawDelta / snap) * snap;

        return rawDelta;
    }

    Vector3 ClampToWorldBounds(Vector3 candidate)
    {
        if (worldBounds.HasValue)
        {
            var b = worldBounds.Value;
            candidate.x = Mathf.Clamp(candidate.x, b.min.x, b.max.x);
            candidate.y = Mathf.Clamp(candidate.y, b.min.y, b.max.y);
            candidate.z = Mathf.Clamp(candidate.z, b.min.z, b.max.z);
        }
        return candidate;
    }
}
