using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandLayout : MonoBehaviour
{
    [SerializeField] private float cardSpacing = 100f;
    [SerializeField] private float cardOverlap = -60f;
    [SerializeField] public float animationDuration { get; private set; } = 0.2f;

    private readonly Dictionary<RectTransform, Coroutine> activeCoroutines = new();
    private bool isRepositioning;

    public bool IsRepositioning => isRepositioning;

    public void RepositionCards()
    {
        if (isRepositioning) return;
        isRepositioning = true;

        List<RectTransform> cards = new();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
            if (child != null)
            {
                cards.Add(child);
            }
        }

        int count = cards.Count;
        if (count == 0)
        {
            isRepositioning = false;
            return;
        }

        float totalWidth = cardSpacing * (count - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = cards[i];
            float x = startX + i * cardSpacing;
            Vector2 targetPos = new(x, 0);

            if (activeCoroutines.TryGetValue(card, out Coroutine old))
            {
                StopCoroutine(old);
                activeCoroutines.Remove(card);
            }

            Coroutine move = StartCoroutine(SmoothMove(card, targetPos));
            activeCoroutines[card] = move;
        }

        StartCoroutine(ResetRepositioningFlag());
    }

    private IEnumerator SmoothMove(RectTransform card, Vector2 targetPos)
    {
        Vector2 start = card.anchoredPosition;
        float t = 0;

        while (t < animationDuration)
        {
            if (card == null) yield break;
            card.anchoredPosition = Vector2.Lerp(start, targetPos, t / animationDuration);
            t += Time.deltaTime;
            yield return null;
        }

        card.anchoredPosition = targetPos;
        activeCoroutines.Remove(card);
    }

    private IEnumerator ResetRepositioningFlag()
    {
        yield return new WaitForSeconds(animationDuration);
        isRepositioning = false;
    }
}