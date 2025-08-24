using UnityEngine.EventSystems;

public abstract class Draggable : Selectable, IDraggable
{
    // --- EventSystem interfaces ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        CastHandler<IDraggableHandler>()?.HandleBeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        CastHandler<IDraggableHandler>()?.HandleDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CastHandler<IDraggableHandler>()?.HandleEndDrag(this, eventData);
    }
}
