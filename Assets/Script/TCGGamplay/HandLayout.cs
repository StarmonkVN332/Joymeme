using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandLayout : MonoBehaviour
{
    [SerializeField] private float cardSpacing = 150f; // Horizontal spacing between cards
    [SerializeField] private float animationDuration = 0.2f; // Animation time
    [SerializeField] private Vector2 positionOffset = new Vector2(0, 50); // Offset to center or adjust cards
    private readonly Dictionary<RectTransform, Coroutine> activeCoroutines = new Dictionary<RectTransform, Coroutine>();
    private bool isRepositioning; // Flag to lock cards during animation

    public float AnimationDuration => animationDuration;

    public void RepositionCards()
    {
        if (isRepositioning)
        {
            Debug.Log("HandLayout: Repositioning already in progress, skipping.");
            return;
        }
        isRepositioning = true;

        int cardCount = transform.childCount;
        float totalWidth = (cardCount - 1) * cardSpacing;
        float startX = -totalWidth / 2;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = transform.GetChild(i).GetComponent<RectTransform>();
            if (card == null)
            {
                Debug.LogWarning($"HandLayout: Child {i} has no RectTransform, skipping.");
                continue;
            }
            // Stop existing coroutine
            if (activeCoroutines.TryGetValue(card, out Coroutine existingCoroutine) && existingCoroutine != null)
            {
                StopCoroutine(existingCoroutine);
                activeCoroutines.Remove(card);
            }
            Vector2 targetPos = new Vector2(startX + i * cardSpacing + positionOffset.x, positionOffset.y);
            // Skip animation if card is close to target
            if (Vector2.Distance(card.anchoredPosition, targetPos) < 1f)
            {
                card.anchoredPosition = targetPos;
                Debug.Log($"HandLayout: Card {card.name} already at {targetPos}, skipping animation.");
                continue;
            }
            Coroutine newCoroutine = StartCoroutine(SmoothMove(card, targetPos));
            activeCoroutines[card] = newCoroutine;
        }

        // Reset flag after animation duration
        StartCoroutine(ResetRepositioningFlag());
    }

    private IEnumerator SmoothMove(RectTransform card, Vector2 targetPos)
    {
        if (card == null)
        {
            yield break;
        }
        Vector2 startPos = card.anchoredPosition;
        Debug.Log($"HandLayout: Moving card {card.name} from {startPos} to {targetPos}");
        float time = 0;
        while (time < animationDuration)
        {
            if (card == null)
            {
                Debug.Log("HandLayout: Card destroyed during SmoothMove.");
                yield break;
            }
            float t = time / animationDuration;
            card.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }
        if (card != null)
        {
            card.anchoredPosition = targetPos;
            Debug.Log($"HandLayout: Card {card.name} reached {targetPos}");
        }
        activeCoroutines.Remove(card);
    }

    private IEnumerator ResetRepositioningFlag()
    {
        yield return new WaitForSeconds(animationDuration);
        isRepositioning = false;
    }

    public bool IsRepositioning => isRepositioning; // For DraggableCard to check
}