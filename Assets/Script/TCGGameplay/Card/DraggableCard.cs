using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

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
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(originalParent.parent); // move to canvas root
        canvasGroup.blocksRaycasts = false;

        // ⛔ No repositioning here!
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

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
                StartCoroutine(SmoothSnapBack(rectTransform.anchoredPosition, originalPosition, handLayout.animationDuration, () =>
                {
                    handLayout.RepositionCards(); // Re-align cards AFTER the card finishes snapping back
                }));
            }
            else
            {
                rectTransform.anchoredPosition = originalPosition;
            }
        }

    }
    private IEnumerator SmoothSnapBack(Vector2 from, Vector2 to, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = to;
        onComplete?.Invoke();
    }

    public void ResetToHand()
    {
        if (originalParent == null) return;

        // Move back to hand visually
        StartCoroutine(SmoothReturnToHand());
    }

    private IEnumerator SmoothReturnToHand()
    {
        // Capture world pos
        Vector3 worldPos = transform.position;

        // Reparent without moving visually
        transform.SetParent(originalParent, true);
        transform.position = worldPos;

        // Animate back to proper anchored position
        RectTransform handSlot = GetComponent<RectTransform>();
        HandLayout handLayout = originalParent.GetComponent<HandLayout>();

        if (handLayout == null || handSlot == null)
            yield break;

        // Wait a frame so Unity lays out the card in the right anchored position
        yield return null;

        Vector2 targetPos = handSlot.anchoredPosition;
        Quaternion targetRot = handSlot.localRotation;

        float duration = handLayout.animationDuration;
        float time = 0f;
        Vector2 startPos = handSlot.anchoredPosition; // This may still be correct
        Quaternion startRot = handSlot.localRotation;

        while (time < duration)
        {
            float t = time / duration;
            handSlot.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            handSlot.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            time += Time.deltaTime;
            yield return null;
        }

        handSlot.anchoredPosition = targetPos;
        handSlot.localRotation = targetRot;

        handLayout.RepositionCards(); // Snap all others now
    }

    public IEnumerator FadeOutAndDestroy(DraggableCard card, float duration = 0.3f)
    {
        if (card == null) yield break;

        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            if (card.gameObject != null)
                Destroy(card.gameObject); // fallback
            yield break;
        }

        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            if (cg == null || card == null) yield break; // extra null checks
            cg.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (cg != null)
            cg.alpha = 0f;

        if (card != null && card.gameObject != null)
            Destroy(card.gameObject);
    }

}
