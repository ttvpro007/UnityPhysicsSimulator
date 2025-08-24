using UnityEngine.EventSystems;

public enum HandleState { Normal, Hover, Active }

public interface ISelectableHandler
{
    void HandlePointerEnter(ISelectable interactible, PointerEventData eventData);

    void HandlePointerExit(ISelectable interactible, PointerEventData eventData);

    void HandlePointerDown(ISelectable interactible, PointerEventData eventData);

    void HandlePointerUp(ISelectable interactible, PointerEventData eventData);
}
