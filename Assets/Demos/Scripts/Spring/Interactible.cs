using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Interactible : MonoBehaviour, IInteractible
{
    [Header("Visuals")]
    public Renderer targetRenderer;                 // If null, auto-find
    public Color normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public Color hoverColor = new Color(1f, 0.9f, 0.3f, 1f);
    public Color activeColor = new Color(0.3f, 1f, 1f, 1f);
    [Range(1f, 2f)] public float hoverScale = 1f;
    [Range(1f, 3f)] public float activeScale = 1f;

    IInteractibleHandler _gizmo;
    Vector3 _initialScale;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _gizmo = GetComponentInParent<IInteractibleHandler>();
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        _initialScale = transform.localScale;
        _mpb = new MaterialPropertyBlock();
        Apply(normalColor, 1f);
    }

    public void SetState(HandleState s)
    {
        switch (s)
        {
            case HandleState.Normal: Apply(normalColor, 1f); break;
            case HandleState.Hover: Apply(hoverColor, hoverScale); break;
            case HandleState.Active: Apply(activeColor, activeScale); break;
        }
    }

    void Apply(Color color, float scaleMul)
    {
        if (targetRenderer)
        {
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_Color", color);
            _mpb.SetColor("_BaseColor", color);
            targetRenderer.SetPropertyBlock(_mpb);
        }
        transform.localScale = _initialScale * scaleMul;
    }

    // --- EventSystem interfaces ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        _gizmo?.OnHandlePointerEnter(this, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _gizmo?.OnHandlePointerExit(this, eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _gizmo?.OnHandlePointerDown(this, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _gizmo?.OnHandlePointerUp(this, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _gizmo?.OnHandleBeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _gizmo?.OnHandleDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _gizmo?.OnHandleEndDrag(this, eventData);
    }
}
