using UnityEngine.EventSystems;

public enum HandleState { Normal, Hover, Active }

public interface IInteractible :
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
}
