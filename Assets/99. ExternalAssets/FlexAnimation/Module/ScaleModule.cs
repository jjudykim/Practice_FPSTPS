using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ScaleModule : AnimationModule
{
    [Header("Values")]
    public Vector3 endValue = Vector3.one;
    public bool relative = false;

    public override Tween CreateTween(Transform target)
    {
        Tween t = target.DOScale(endValue, duration);
        if (relative) t.SetRelative(true);
        return t;
    }
}
