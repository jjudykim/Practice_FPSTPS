using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class PunchModule : AnimationModule
{
    [Header("Values")]
    public Vector3 punch = new Vector3(0.5f, 0.5f, 0.5f);
    public int vibrato = 10;
    public float elasticity = 1f;

    public override Tween CreateTween(Transform target)
    {
        return target.DOPunchScale(punch, duration, vibrato, elasticity);
    }
}
