using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(10000)]
public class RuntimeTransform : MonoBehaviour, IInteractibleHandler
{
    [Header("Setup")]
    public Camera sceneCamera;

    [Header("Behavior")]
    public bool useLocalSpace = false;   // world- or local-aligned axes
    public bool autoSize = true;
    [Range(0.01f, 1f)] public float sizeFactor = 0.15f;
    public float snapStep = 0f;

    [Header("State (read-only)")]
    public Transform target;
    public RuntimeTransformAxis hovered;
    public RuntimeTransformAxis active;
    public bool isDragging;

    RuntimeTransformAxis[] _handles;
    Vector3 _dragAxisWorld;
    Vector3 _targetStartPos;
    Vector3 _dragPlaneOrigin;
    Plane _dragPlane;
    int _activePointerId = -999;

    void Awake()
    {
        if (!sceneCamera) sceneCamera = Camera.main;
        _handles = GetComponentsInChildren<RuntimeTransformAxis>(includeInactive: true);
        SetAllStates(HandleState.Normal);
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

    // ---------- Public control ----------
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

    // ---------- Handle event entry points (called by RuntimeTransformAxis) ----------
    public void OnHandlePointerEnter(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return; // ignore if not runtime transform axis

        if (!target || isDragging) return; // ignore hover while dragging

        if (hovered != rta)
        {
            SetAllStates(HandleState.Normal);
            rta.SetState(HandleState.Hover);
            hovered = (RuntimeTransformAxis) interactible;
        }
    }

    public void OnHandlePointerExit(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return; // ignore if not runtime transform axis

        if (!target || isDragging) return; // ignore hover while dragging

        if (hovered == rta)
        {
            rta.SetState(HandleState.Normal);
            hovered = null;
        }
    }

    public void OnHandlePointerDown(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis) return; // ignore if not runtime transform axis

        // Only left mouse/touch primary
        if (!target) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _activePointerId = eventData.pointerId;
    }

    public void OnHandlePointerUp(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis) return; // ignore if not runtime transform axis

        if (eventData.pointerId != _activePointerId) return;
        _activePointerId = -999;
        // If pointer up without a drag, nothing else to do.
    }

    public void OnHandleBeginDrag(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis) return; // ignore if not runtime transform axis

        if (!target) return;
        if (eventData.pointerId != _activePointerId) return;

        active = (RuntimeTransformAxis) interactible;
        isDragging = true;
        _targetStartPos = target.position;

        // Axis in world
        _dragAxisWorld = GetWorldAxis(active.axis).normalized;

        // Plane that contains the axis and faces the camera
        Camera cam = EventCamera(eventData);
        Vector3 camFwd = cam ? cam.transform.forward : Vector3.forward;
        Vector3 n = Vector3.ProjectOnPlane(camFwd, _dragAxisWorld);
        if (n.sqrMagnitude < 1e-6f)
            n = Vector3.ProjectOnPlane((cam ? cam.transform.up : Vector3.up), _dragAxisWorld);
        if (n.sqrMagnitude < 1e-6f) n = Vector3.right;
        n.Normalize();

        _dragPlane = new Plane(n, _targetStartPos);

        // Starting point
        Ray r = EventRay(eventData);
        _dragPlaneOrigin = RayToPlane(r, _dragPlane, _targetStartPos);

        SetAllStates(HandleState.Normal);
        active.SetState(HandleState.Active);
    }

    public void OnHandleDrag(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return; // ignore if not runtime transform axis

        if (!target || !isDragging || active != rta) return;
        Ray r = EventRay(eventData);

        Vector3 currOnPlane = RayToPlane(r, _dragPlane, _dragPlaneOrigin);
        float delta = Vector3.Dot(currOnPlane - _dragPlaneOrigin, _dragAxisWorld);

        if (snapStep > 0f)
            delta = Mathf.Round(delta / snapStep) * snapStep;

        target.position = _targetStartPos + _dragAxisWorld * delta;
    }

    public void OnHandleEndDrag(IInteractible interactible, PointerEventData eventData)
    {
        if (interactible is not RuntimeTransformAxis rta) return; // ignore if not runtime transform axis

        if (active != rta) return;
        isDragging = false;
        _activePointerId = -999;

        // If still hovering, return to hover state; else normal
        if (hovered == rta) rta.SetState(HandleState.Hover);
        else rta.SetState(HandleState.Normal);

        active = null;
    }

    // ---------- Helpers ----------
    Vector3 GetWorldAxis(Axis a)
    {
        return a switch
        {
            Axis.X => transform.right,
            Axis.Y => transform.up,
            Axis.Z => transform.forward,
            _ => Vector3.zero,
        };
    }

    void SetAllStates(HandleState s)
    {
        if (_handles == null) return;
        foreach (var h in _handles) if (h) h.SetState(s);
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
        // Fallback (very rare): default forward ray from camera-less world origin
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

        if (Mathf.Abs(D) < 1e-6f) return 0f;
        return (b * e - d) / D;
    }
}
