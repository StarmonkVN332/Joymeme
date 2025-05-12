using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public Transform spawnPoint;
    public GameObject unitPrefab;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableCard draggedCard = eventData.pointerDrag.GetComponent<DraggableCard>();
        if (draggedCard != null && draggedCard.cardData.cardType == CardType.Unit)
        {
            Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
            Destroy(draggedCard.gameObject); // remove from hand
        }
    }
}
