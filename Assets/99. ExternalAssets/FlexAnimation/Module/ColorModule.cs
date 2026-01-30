using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class ColorModule : AnimationModule
{
    [Header("Values")]
    public Color color = Color.white;
    public bool useAlphaOnly;
    public float alpha = 1f;

    public override Tween CreateTween(Transform target)
    {
        Color targetColor = color;

        if (target.TryGetComponent(out Graphic graphic))
        {
            if (useAlphaOnly) return graphic.DOFade(alpha, duration);
            return graphic.DOColor(targetColor, duration);
        }

        if (target.TryGetComponent(out SpriteRenderer sr))
        {
            if (useAlphaOnly) return sr.DOFade(alpha, duration);
            return sr.DOColor(targetColor, duration);
        }

        if (target.TryGetComponent(out CanvasGroup cg))
        {
             return cg.DOFade(useAlphaOnly ? alpha : targetColor.a, duration);
        }

        return null;
    }
}
