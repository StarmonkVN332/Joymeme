using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;
    [SerializeField] private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private RectTransform rectTransform;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
        {
            Debug.LogError("DraggableCard: CanvasGroup component missing!");
        }
        if (rectTransform == null)
        {
            Debug.LogError("DraggableCard: RectTransform component missing!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("DraggableCard: cardData is null!");
            return;
        }

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        DropZone dropZone = null;
        if (eventData.pointerEnter != null)
        {
            dropZone = eventData.pointerEnter.GetComponent<DropZone>();
        }

        if (dropZone != null && dropZone.CanPlayCard(cardData))
        {
            dropZone.OnDrop(eventData);
        }
        else
        {
            if (transform.parent == originalParent)
            {
                HandLayout handLayout = originalParent.GetComponent<HandLayout>();
                if (handLayout != null && handLayout.IsRepositioning)
                {
                    // Delay reset until repositioning finishes
                    StartCoroutine(ResetPositionAfterRepositioning(handLayout));
                }
                else
                {
                    rectTransform.anchoredPosition = originalPosition;
                    Debug.Log($"DraggableCard: Reset {gameObject.name} to {originalPosition}");
                }
            }
        }
    }

    private IEnumerator ResetPositionAfterRepositioning(HandLayout handLayout)
    {
        while (handLayout.IsRepositioning)
        {
            yield return null;
        }
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
            Debug.Log($"DraggableCard: Delayed reset {gameObject.name} to {originalPosition}");
        }
    }
}