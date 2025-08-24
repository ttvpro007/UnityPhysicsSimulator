using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Selectable : MonoBehaviour, ISelectable
{
    [Header("Visuals")]
    public Renderer targetRenderer;                 // If null, auto-find
    public Color normalColor = new(0.85f, 0.85f, 0.85f, 1f);
    public Color hoverColor = new(1f, 0.9f, 0.3f, 1f);
    public Color activeColor = new(0.3f, 1f, 1f, 1f);
    [Range(1f, 2f)] public float hoverScale = 1f;
    [Range(1f, 3f)] public float activeScale = 1f;

    ISelectableHandler _handler;
    Vector3 _initialScale;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _handler = GetComponentInParent<ISelectableHandler>();
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

    protected T CastHandler<T>() where T : ISelectableHandler
    {
        if (_handler == null)
            return default;

        return (T)_handler;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _handler?.HandlePointerDown(this, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _handler?.HandlePointerEnter(this, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _handler?.HandlePointerExit(this, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _handler?.HandlePointerUp(this, eventData);
    }
}
