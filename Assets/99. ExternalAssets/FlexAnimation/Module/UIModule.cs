using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class UIModule : AnimationModule
{
    [Header("Values")]
    public bool anchorPos;
    public Vector2 anchorPosValue;
    
    public bool sizeDelta;
    public Vector2 sizeDeltaValue;
    
    public bool relative;

    public override Tween CreateTween(Transform target)
    {
        if (!target.TryGetComponent(out RectTransform rect)) return null;

        Sequence seq = DOTween.Sequence();

        if (anchorPos)
        {
            Tween t = rect.DOAnchorPos(anchorPosValue, duration);
            if (relative) t.SetRelative(true);
            seq.Join(t);
        }

        if (sizeDelta)
        {
            Tween t = rect.DOSizeDelta(sizeDeltaValue, duration);
            if (relative) t.SetRelative(true);
            seq.Join(t);
        }

        return seq;
    }
}
