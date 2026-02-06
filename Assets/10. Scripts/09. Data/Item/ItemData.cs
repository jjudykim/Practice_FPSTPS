using System;
using UnityEngine;

[Serializable]
public class ItemEffectData
{
    public EffectType EffectType = EffectType.None;

    // Heal / Buff 공용 값 (기획에 따라 분리해도 됨)
    public float EffectValue = 0f;

    // 0이면 즉시 효과로 간주(예: 즉시 힐)
    public float EffectDuration = 0f;

    // 0이면 "한 번만 적용"으로 간주할지, "매 프레임"으로 할지 정책 필요
    // 초안에서는 0이면 Tick 없음(즉시)으로 간주
    public float EffectTickRate = 0f;

    public void ValidateAndClamp()
    {
        if (EffectValue < 0f) EffectValue = 0f;
        if (EffectDuration < 0f) EffectDuration = 0f;
        if (EffectTickRate < 0f) EffectTickRate = 0f;

        // None이면 나머지 값 의미 없음
        if (EffectType == EffectType.None)
        {
            EffectValue = 0f;
            EffectDuration = 0f;
            EffectTickRate = 0f;
        }
    }
}

[Serializable]
public class ItemData
{
    public string Id;
    public string DisplayName;
    public string Description;

    public ItemType Type = ItemType.Consumable;

    // 중복 슬롯 가능
    public EquipSlotFlags SlotFlags = EquipSlotFlags.General;

    // 1개당 무게
    public float Weight = 0f;

    // 중첩 가능 수
    public int StackLimit = 1;

    public ItemEffectData Effect = new ItemEffectData();

    public void ValidateAndClamp()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new Exception("[ItemDefinition] Id is null/empty.");

        if (string.IsNullOrWhiteSpace(DisplayName))
            DisplayName = Id;

        if (Weight < 0f)
            Weight = 0f;

        if (StackLimit <= 0)
            StackLimit = 1;

        // 요구사항: 모든 아이템은 General 기본 포함
        SlotFlags |= EquipSlotFlags.General;

        // Type이 Consumable이 아닌데 Effect가 있다면? (가능/불가 정책)
        // 초안에서는 허용(장착 시 Buff 같은 경우)
        Effect?.ValidateAndClamp();

        // Stack 정책:
        // - Weapon/Equipment는 보통 1이 자연스럽지만, 기획상 스택형 장비도 가능할 수 있으니 강제하진 않음.
        // - 다만 Equip 대상이면 1이 편함 → 추후 기획 확정되면 여기서 강제 가능.
    }

    public bool CanEquipTo(EquipSlotFlags slot)
    {
        return (SlotFlags & slot) != 0;
    }
}
