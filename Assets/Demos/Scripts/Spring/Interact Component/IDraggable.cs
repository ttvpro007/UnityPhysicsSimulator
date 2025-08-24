using UnityEngine.EventSystems;

public interface IDraggable : ISelectable,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
}
