using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDisappear : MonoBehaviour
{
      [SerializeField] private Material disappearMat; // gán material "DisappearCardMat" ở Inspector

    public void TriggerDisappearSmooth(float duration = 1f)
    {
        StartCoroutine(DissolveOverTime(duration));
    }

    public IEnumerator DissolveOverTime(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(0f, 1f, elapsed / duration);
            disappearMat.SetFloat("_DissolveAmount", dissolveValue);
            yield return null;
        }
    }
}
