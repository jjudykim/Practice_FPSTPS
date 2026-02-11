using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsFading { get; private set; }

    public IEnumerator CoFade(float targetAlpha, float duration)
    {
        IsFading = true;
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        IsFading = false;
    }

    public void SetAlpha(float alpha) => canvasGroup.alpha = alpha;
}