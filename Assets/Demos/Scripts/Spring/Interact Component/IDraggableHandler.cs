using UnityEngine.EventSystems;

public interface IDraggableHandler : ISelectableHandler
{
    void HandleBeginDrag(IDraggable draggable, PointerEventData eventData);

    void HandleDrag(IDraggable draggable, PointerEventData eventData);

    void HandleEndDrag(IDraggable draggable, PointerEventData eventData);
}
