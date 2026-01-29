using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class MoveModule : AnimationModule
{
    [Header("Values")]
    public bool x, y, z;
    public Vector3 endValue;
    public bool relative = false;

    public override Tween CreateTween(Transform target)
    {
        Vector3 start = target.position;
        Vector3 dest = new(
            x ? endValue.x : start.x,
            y ? endValue.y : start.y,
            z ? endValue.z : start.z
        );

        Tween t = target.DOMove(dest, duration);
        if (relative) t.SetRelative(true);
        return t;
    }
}