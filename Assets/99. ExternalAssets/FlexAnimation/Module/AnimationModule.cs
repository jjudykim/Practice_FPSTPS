using UnityEngine;
using System;
using DG.Tweening;

public enum LoopMode { None, Loop, Yoyo }

[Serializable]
public abstract class AnimationModule
{
    [Header("Behavior")]
    public bool enabled = true;
    public FlexLinkType linkType = FlexLinkType.Append;
    public float delay; // Append: 대기 시간, Join: 시작 딜레이

    [Header("Settings")]
    public float duration = 0.5f;
    public AnimEase ease = AnimEase.OutQuad;
    public LoopMode loop = LoopMode.None;
    public int loopCount = -1; // -1: Infinite (if LoopMode is not None)

    public virtual Tween CreateTween(Transform target)
    {
        return null;
    }

    protected Ease GetEase()
    {
        if (Enum.TryParse(ease.ToString(), out Ease result))
        {
            return result;
        }
        return Ease.Linear;
    }

    public void ApplyCommonSettings(Tween t)
    {
        if (t == null) return;

        t.SetEase(GetEase());

        switch (loop)
        {
            case LoopMode.Loop:
                t.SetLoops(loopCount < 0 ? -1 : loopCount, LoopType.Restart);
                break;
            case LoopMode.Yoyo:
                t.SetLoops(loopCount < 0 ? -1 : loopCount, LoopType.Yoyo);
                break;
        }
    }
}