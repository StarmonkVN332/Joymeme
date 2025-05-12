using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    public float spacing = 150f;
    public float animationSpeed = 5f;

    void Update()
    {
        AlignCards();
    }

    void AlignCards()
    {
        int count = transform.childCount;
        float totalWidth = (count - 1) * spacing;
        for (int i = 0; i < count; i++)
        {
            Transform card = transform.GetChild(i);
            Vector3 targetPos = new Vector3((i * spacing) - totalWidth / 2f, 0f, 0f);
            card.localPosition = Vector3.Lerp(card.localPosition, targetPos, Time.deltaTime * animationSpeed);
        }
    }
}
