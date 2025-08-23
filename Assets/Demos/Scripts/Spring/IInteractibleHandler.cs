using UnityEngine.EventSystems;

public interface IInteractibleHandler
{
    void OnHandlePointerEnter(IInteractible interactible, PointerEventData eventData);

    void OnHandlePointerExit(IInteractible interactible, PointerEventData eventData);

    void OnHandlePointerDown(IInteractible interactible, PointerEventData eventData);

    void OnHandlePointerUp(IInteractible interactible, PointerEventData eventData);

    void OnHandleBeginDrag(IInteractible interactible, PointerEventData eventData);

    void OnHandleDrag(IInteractible interactible, PointerEventData eventData);

    void OnHandleEndDrag(IInteractible interactible, PointerEventData eventData);
}
