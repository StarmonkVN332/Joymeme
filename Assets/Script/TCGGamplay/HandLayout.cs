using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    [SerializeField] private float cardSpacing = 150f;
    [SerializeField] private float animationDuration = 0.2f;

    public void RepositionCards()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform card = transform.GetChild(i).GetComponent<RectTransform>();
            Vector3 targetPos = new Vector3(i * cardSpacing, 0, 0);
            StartCoroutine(SmoothMove(card, targetPos));
        }
    }

    IEnumerator SmoothMove(RectTransform card, Vector3 targetPos)
    {
        Vector3 startPos = card.anchoredPosition;
        float time = 0;
        while (time < animationDuration)
        {
            card.anchoredPosition = Vector3.Lerp(startPos, targetPos, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        card.anchoredPosition = targetPos;
    }
}
