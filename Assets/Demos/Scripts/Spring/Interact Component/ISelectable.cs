using UnityEngine.EventSystems;

public interface ISelectable :
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
}
