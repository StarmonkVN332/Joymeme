using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;
using DG.Tweening;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;
    private CanvasGroup canvasGroup;
    public Transform originalParent { get; private set; }
    private int originalSiblingIndex;
    private Vector2 originalPosition;
    private RectTransform rectTransform;
    private bool isGrayedOut = false;
    public bool HasBeenDropped { get; private set; } = false;
    private Vector2 pointerOffset;

    public void MarkAsDropped()
    {
        HasBeenDropped = true;
    }

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            Debug.LogError($"[{name}] CanvasGroup missing!");
        if (rectTransform == null)
            Debug.LogError($"[{name}] RectTransform missing!");
    }

    public void SetGrayOut(bool grayOut)
    {
        isGrayedOut = grayOut;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isGrayedOut ? 0.5f : 1f;
            canvasGroup.interactable = !isGrayedOut;
        }
    }
    public bool IsGrayedOut()
    {
        return isGrayedOut;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        CardClickZoom zoom = GetComponent<CardClickZoom>();
        if (zoom != null && zoom.savedSiblingIndex >= 0)
            originalSiblingIndex = zoom.savedSiblingIndex;
        else
            originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(originalParent.parent); // move to canvas root
        canvasGroup.blocksRaycasts = false;

        // Calculate offset from mouse position to the rectTransform center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            // Apply offset so the card doesn’t snap to center
            rectTransform.localPosition = localPoint - pointerOffset;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (HasBeenDropped)
        {
            Debug.Log($"[{name}] Card was successfully dropped. Skipping snap-back.");
            StartCoroutine(FadeOutAndDestroy(this));
            return;
        }
        DropZone dropZone = eventData.pointerEnter?.GetComponent<DropZone>();

        if (dropZone != null && dropZone.CanPlayCard(cardData) &&
            ManaManager.Instance.HasEnoughMana(PhotonNetwork.LocalPlayer.ActorNumber, cardData.manaCost))
        {
            Debug.Log($"[{name}] Dropped on valid drop zone: {dropZone.name}");
            dropZone.OnDrop(eventData); 

            // ✅ Only realign after card is played
            HandLayout handLayout = originalParent.GetComponent<HandLayout>();
            handLayout?.RepositionCards();
        }
        else
        {
            Debug.Log($"[{name}] Invalid drop, snapping back");

            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);

            // 👇 Smoothly animate back to original position
            HandLayout handLayout = originalParent.GetComponent<HandLayout>();
            if (handLayout != null)
            {
                SnapBackToOriginal(rectTransform.anchoredPosition, originalPosition, handLayout.animationDuration, () =>
                {
                    handLayout.RepositionCards(); // Align after animation
                });
            }
            else
            {
                rectTransform.anchoredPosition = originalPosition;
            }
        }

    }
    private void SnapBackToOriginal(Vector2 from, Vector2 to, float duration, System.Action onComplete)
    {
        rectTransform.DOAnchorPos(to, duration).SetEase(Ease.OutCubic).OnComplete(() => onComplete?.Invoke());
    }


    public void ResetToHand()
    {
        if (originalParent == null) return;

        // Move back to hand visually
        StartCoroutine(SmoothReturnToHand());
    }

    private IEnumerator SmoothReturnToHand()
    {
        Vector3 worldPos = transform.position;
        transform.SetParent(originalParent, true);
        transform.position = worldPos;

        yield return null;

        RectTransform handSlot = GetComponent<RectTransform>();
        HandLayout handLayout = originalParent.GetComponent<HandLayout>();
        if (handLayout == null || handSlot == null) yield break;

        Vector2 targetPos = handSlot.anchoredPosition;
        Quaternion targetRot = handSlot.localRotation;

        handSlot.DOAnchorPos(targetPos, handLayout.animationDuration).SetEase(Ease.OutCubic);
        handSlot.DOLocalRotateQuaternion(targetRot, handLayout.animationDuration).SetEase(Ease.OutCubic)
            .OnComplete(() => handLayout.RepositionCards());
    }


    public IEnumerator FadeOutAndDestroy(DraggableCard card, float duration = 0.3f)
    {
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Destroy(card.gameObject);
            yield break;
        }

        cg.DOFade(0f, duration).OnComplete(() =>
        {
            if (card != null && card.gameObject != null)
                Destroy(card.gameObject);
        });

        yield return new WaitForSeconds(duration);
    }


}
