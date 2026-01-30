using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RotateModule : AnimationModule
{
    [Header("Values")]
    public Vector3 endValue;
    public bool relative;

    public override Tween CreateTween(Transform target)
    {
        RotateMode mode = loop == LoopMode.None ? RotateMode.Fast : RotateMode.FastBeyond360; 
        // Note: Logic simplified for general usage. Infinite loops usually prefer FastBeyond360.
        // But users can override via loopCount.

        Tween t = target.DOLocalRotate(endValue, duration, mode);
        if (relative) t.SetRelative(true);
        return t;
    }
}