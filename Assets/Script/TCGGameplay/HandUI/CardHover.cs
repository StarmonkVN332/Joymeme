using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float scaleSpeed = 8f;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(originalScale * hoverScale));
        transform.SetAsLastSibling(); // Optional: still bring to front
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(rectTransform.localScale, targetScale) > 0.01f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }
}
