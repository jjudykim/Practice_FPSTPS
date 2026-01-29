using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening;

public enum AnimEase
{
    Linear,
    InSine, OutSine, InOutSine,
    InQuad, OutQuad, InOutQuad,
    InCubic, OutCubic, InOutCubic,
    InQuart, OutQuart, InOutQuart,
    InQuint, OutQuint, InOutQuint,
    InExpo, OutExpo, InOutExpo,
    InCirc, OutCirc, InOutCirc,
    InElastic, OutElastic, InOutElastic,
    InBack, OutBack, InOutBack,
    InBounce, OutBounce, InOutBounce
}

public class FlexAnimation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private FlexAnimationPreset preset;

    [Header("Override")]
    [SerializeField] private float timeScale = 1f;
    [SerializeField] private bool ignoreTimeScale = false;

    [Header("Events")]
    public UnityEvent OnPlay;
    public UnityEvent OnComplete;

    [SerializeReference]
    public List<AnimationModule> modules = new List<AnimationModule>();

    private Sequence currentSequence;

    private void OnEnable()
    {
        if (playOnEnable)
            PlayAll();
    }

    private void OnDisable()
    {
        StopAll();
    }

    public void PlayAll()
    {
        StopAll(); // Kill existing

        OnPlay?.Invoke();

        List<AnimationModule> targetModules = (preset != null) ? preset.modules : modules;
        if (targetModules == null || targetModules.Count == 0) return;

        currentSequence = DOTween.Sequence();
        currentSequence.SetUpdate(ignoreTimeScale);
        currentSequence.timeScale = timeScale;

        foreach (var module in targetModules)
        {
            if (!module.enabled) continue;

            Tween t = module.CreateTween(transform);
            if (t == null) continue; // Skip empty modules

            module.ApplyCommonSettings(t);

            // Sequencing Logic
            switch (module.linkType)
            {
                case FlexLinkType.Append:
                    if (module.delay > 0) currentSequence.AppendInterval(module.delay);
                    currentSequence.Append(t);
                    break;

                case FlexLinkType.Join:
                    if (module.delay > 0) t.SetDelay(module.delay); // Join delay is strictly localized
                    currentSequence.Join(t);
                    break;

                case FlexLinkType.Insert:
                    currentSequence.Insert(module.delay, t);
                    break;
            }
        }

        currentSequence.OnComplete(() => OnComplete?.Invoke());
        currentSequence.Play();
    }

    public void StopAll()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        currentSequence = null;
    }
}