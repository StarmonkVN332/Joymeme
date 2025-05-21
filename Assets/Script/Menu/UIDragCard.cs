using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
   [SerializeField] public Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField][HideInInspector] public bool isDroppedOnZone = false;
    private Vector3 originalPosition;
    private Transform originalParent;

    private bool isDroppedZone = false;

    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        isDroppedZone = false;

        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!isDroppedZone)
        {
            StartCoroutine(SmoothReturnWithShake());
        }
    }

    public void MarkAsDropped(Vector3 dropPosition)
    {
        isDroppedZone = true;
        StartCoroutine(SmoothPlaceIntoZone(dropPosition));
        if (dropSound) audioSource.PlayOneShot(dropSound);
    }

    private IEnumerator SmoothReturnWithShake()
    {
        float t = 0;
        Vector3 start = rectTransform.position;
        float shakeAmount = 5f;

        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            Vector3 targetPos = Vector3.Lerp(start, originalPosition, t);
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * shakeAmount * (1 - t);
            rectTransform.position = targetPos + offset;
            yield return null;
        }

        rectTransform.position = originalPosition;
        transform.SetParent(originalParent);
    }

    private IEnumerator SmoothPlaceIntoZone(Vector3 targetPos)
    {
        float t = 0;
        Vector3 start = rectTransform.position;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            rectTransform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        rectTransform.position = targetPos;
    }
}