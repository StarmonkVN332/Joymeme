using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandLayout : MonoBehaviour
{
    [SerializeField] private float maxTotalWidth = 1000f; // Max horizontal spread
    [SerializeField] private float cardSpacing = 150f; // Ideal spacing if few cards
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Vector2 positionOffset = new Vector2(0, 50); // Adjust vertical center
    [SerializeField] private float curveFactor = 10f; // Controls how curved the fan is
    [SerializeField] private float rotationAngle = 10f; // Max rotation at outermost cards

    private readonly Dictionary<RectTransform, Coroutine> activeCoroutines = new Dictionary<RectTransform, Coroutine>();
    private bool isRepositioning;

    public float AnimationDuration => animationDuration;
    public bool IsRepositioning => isRepositioning;

    public void RepositionCards()
    {
        Debug.Log($"RepositionCards called. Children count: {transform.childCount}");
        if (isRepositioning)
        {
            Debug.Log("HandLayout: Repositioning already in progress, skipping.");
            return;
        }
        isRepositioning = true;

        int cardCount = transform.childCount;
        if (cardCount == 0)
        {
            isRepositioning = false;
            return;
        }

        float spacing = Mathf.Min(cardSpacing, cardCount > 1 ? maxTotalWidth / (cardCount - 1) : 0);
        float totalWidth = spacing * (cardCount - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = transform.GetChild(i).GetComponent<RectTransform>();
            if (card == null)
            {
                Debug.LogWarning($"HandLayout: Child {i} has no RectTransform, skipping.");
                continue;
            }

            float x = startX + i * spacing + positionOffset.x;
            float y = -Mathf.Abs((i - (cardCount - 1) / 2f)) * curveFactor + positionOffset.y;
            float angle = -(i - (cardCount - 1) / 2f) * rotationAngle;

            Vector2 targetPos = new Vector2(x, y);
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);

            // Stop existing coroutine if needed
            if (activeCoroutines.TryGetValue(card, out Coroutine existingCoroutine) && existingCoroutine != null)
            {
                StopCoroutine(existingCoroutine);
                activeCoroutines.Remove(card);
            }

            if (Vector2.Distance(card.anchoredPosition, targetPos) < 1f)
            {
                card.anchoredPosition = targetPos;
                card.localRotation = targetRot;
                continue;
            }

            Coroutine newCoroutine = StartCoroutine(SmoothMove(card, targetPos, targetRot));
            activeCoroutines[card] = newCoroutine;
        }

        StartCoroutine(ResetRepositioningFlag());
    }

    private IEnumerator SmoothMove(RectTransform card, Vector2 targetPos, Quaternion targetRot)
    {
        if (card == null) yield break;

        Vector2 startPos = card.anchoredPosition;
        Quaternion startRot = card.localRotation;
        float time = 0;

        while (time < animationDuration)
        {
            if (card == null) yield break;

            float t = time / animationDuration;
            card.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            card.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            time += Time.deltaTime;
            yield return null;
        }

        if (card != null)
        {
            card.anchoredPosition = targetPos;
            card.localRotation = targetRot;
        }

        activeCoroutines.Remove(card);
    }

    private IEnumerator ResetRepositioningFlag()
    {
        yield return new WaitForSeconds(animationDuration);
        isRepositioning = false;
    }
}
