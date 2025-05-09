using UnityEngine;
using UnityEngine.EventSystems;

public class LaneClickable : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardPlacementManager.Instance.selectedCard != null)
        {
            CardPlacementManager.Instance.TryPlaceSelectedCard(transform);
            Debug.Log("Placed on lane: " + gameObject.name);
        }
    }
}
