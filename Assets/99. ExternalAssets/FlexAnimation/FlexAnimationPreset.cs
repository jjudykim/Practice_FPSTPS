using UnityEngine;
using System.Collections.Generic;

public enum FlexLinkType
{
    Append, // 이전 동작이 끝나고 실행 (순차)
    Join,   // 이전 동작과 함께 실행 (동시)
    Insert  // 특정 시간에 실행
}

public enum FlexValueType
{
    Constant,
    RandomRange
}

[CreateAssetMenu(fileName = "New FlexPreset", menuName = "FlexAnimation/Preset")]
public class FlexAnimationPreset : ScriptableObject
{
    [SerializeReference]
    public List<AnimationModule> modules = new List<AnimationModule>();
}
