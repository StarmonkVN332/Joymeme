using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private RectTransform rectTransform;

    private bool isGrayedOut = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
        {
            Debug.LogError($"[{name}] DraggableCard: CanvasGroup component missing! Please add one.");
        }
        if (rectTransform == null)
        {
            Debug.LogError($"[{name}] DraggableCard: RectTransform component missing!");
        }
    }

    public void SetGrayOut(bool grayOut)
    {
        isGrayedOut = grayOut;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isGrayedOut ? 0.5f : 1f;
            canvasGroup.interactable = !isGrayedOut; // Optional: you can keep this true if interaction is needed
            Debug.Log($"[{name}] SetGrayOut called - GrayOut: {isGrayedOut}, Alpha: {canvasGroup.alpha}");
        }
        else
        {
            Debug.LogWarning($"[{name}] SetGrayOut failed - CanvasGroup is null.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null)
        {
            Debug.LogWarning($"[{name}] DraggableCard: cardData is null!");
            return;
        }

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        Debug.Log($"[{name}] OnBeginDrag - Position: {originalPosition}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        Debug.Log($"[{name}] OnDrag - MousePosition: {Input.mousePosition}");
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
            Debug.Log($"[{name}] OnEndDrag - Dropped on valid drop zone: {dropZone.name}");
            dropZone.OnDrop(eventData);
        }
        else
        {
            Debug.Log($"[{name}] OnEndDrag - Invalid drop, returning to hand");

            if (transform.parent == originalParent)
            {
                HandLayout handLayout = originalParent.GetComponent<HandLayout>();
                if (handLayout != null && handLayout.IsRepositioning)
                {
                    Debug.Log($"[{name}] Waiting for hand to finish repositioning...");
                    StartCoroutine(ResetPositionAfterRepositioning(handLayout));
                }
                else
                {
                    rectTransform.anchoredPosition = originalPosition;
                    Debug.Log($"[{name}] Reset to original position: {originalPosition}");
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
            Debug.Log($"[{name}] Delayed reset to original position: {originalPosition}");
        }
    }
}
