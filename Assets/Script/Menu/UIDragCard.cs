using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
IPointerEnterHandler, IPointerExitHandler
{
    [Header("Canvas & Drag")]
    [SerializeField] public Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject rainbowRingEffect;


    [HideInInspector] public bool isDroppedOnZone = false;

    private Vector3 originalPosition;
    private Transform originalParent;

    [Header("Return Settings")]
    [SerializeField] private float returnSpeed = 5f;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip pickSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Hover Hiệu Ứng")]
    [SerializeField] private float hoverHeight = 20f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private GameObject highlightEffect;

    private Vector3 originalScale;
    private bool isDroppedZone = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        originalScale = transform.localScale;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canvasGroup.blocksRaycasts)
        {
            rectTransform.anchoredPosition += new Vector2(0, hoverHeight);
            transform.localScale = originalScale * hoverScale;
            if (highlightEffect != null)
                highlightEffect.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canvasGroup.blocksRaycasts)
        {
            rectTransform.anchoredPosition -= new Vector2(0, hoverHeight);
            transform.localScale = originalScale;
            if (highlightEffect != null)
                highlightEffect.SetActive(false);
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

        if (pickSound)
            audioSource.PlayOneShot(pickSound);

        if (highlightEffect != null)
            highlightEffect.SetActive(false);
        if (rainbowRingEffect != null)
        {
            rainbowRingEffect.SetActive(true);
            var ps = rainbowRingEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
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
        if (rainbowRingEffect != null)
        {
            rainbowRingEffect.SetActive(false);
        }
    }

    public void MarkAsDropped(Vector3 dropPosition)
    {
        isDroppedZone = true;
        StartCoroutine(SmoothPlaceIntoZone(dropPosition));
        if (dropSound)
            audioSource.PlayOneShot(dropSound);
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
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0)    * shakeAmount * (1 - t);
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