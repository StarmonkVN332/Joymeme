using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public int laneIndex;
    [SerializeField] private Lane3D correspondingLane;

    public void LinkToLane(Lane3D lane)
    {
        correspondingLane = lane;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var card = eventData.pointerDrag?.GetComponent<DraggableCard>();
        if (card != null && correspondingLane != null)
        {
            Instantiate(card.cardData.unitPrefab, correspondingLane.transform.position, Quaternion.identity);
        }
    }
}
