using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class FadeModule : AnimationModule
{
    [Header("Values")]
    public float endAlpha = 1f;

    public override Tween CreateTween(Transform target)
    {
        if (target.TryGetComponent(out CanvasGroup cg)) 
            return cg.DOFade(endAlpha, duration);

        if (target.TryGetComponent(out Graphic gr))
            return gr.DOFade(endAlpha, duration);

        if (target.TryGetComponent(out SpriteRenderer sr)) 
            return sr.DOFade(endAlpha, duration);
            
        return null;
    }
}
