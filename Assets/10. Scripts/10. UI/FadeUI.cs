using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public IEnumerator CoFade(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = true;
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0.01f;
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0.01f;
    }
}