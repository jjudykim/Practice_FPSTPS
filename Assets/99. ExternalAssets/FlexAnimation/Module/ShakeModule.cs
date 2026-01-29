using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ShakeModule : AnimationModule
{
    [Header("Values")]
    public float strength = 1f;
    public int vibrato = 10;

    public override Tween CreateTween(Transform target)
    {
        return target.DOShakePosition(duration, strength, vibrato);
    }
}
