using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardClickZoom : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private int originalSiblingIndex;
    private Coroutine scaleRoutine;

    [SerializeField] private float zoomScale = 1.15f;
    [SerializeField] private float scaleSpeed = 8f;

    private static CardClickZoom currentlyZoomedCard = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentlyZoomedCard != null && currentlyZoomedCard != this)
        {
            currentlyZoomedCard.ResetZoom();
        }

        if (currentlyZoomedCard == this)
        {
            ResetZoom();
        }
        else
        {
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
            StartScaleAnimation(originalScale * zoomScale);
            currentlyZoomedCard = this;
        }
    }

    private void Update()
    {
        if (currentlyZoomedCard == this && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                ResetZoom();
            }
        }
    }

    private void StartScaleAnimation(Vector3 target)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(target));
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

    private void ResetZoom()
    {
        StartScaleAnimation(originalScale);
        transform.SetSiblingIndex(originalSiblingIndex);
        if (currentlyZoomedCard == this)
            currentlyZoomedCard = null;
    }
}
