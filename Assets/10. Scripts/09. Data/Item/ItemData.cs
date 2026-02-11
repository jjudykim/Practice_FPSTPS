using System;
using UnityEngine;

[Serializable]
public class ItemEffectData
{
    public EffectType EffectType = EffectType.None;
    public float EffectValue = 0f;            // Heal / Buff 공용 값
    public float EffectDuration = 0f;         // 0이면 즉시 효과로 간주(예: 즉시 힐)
    public float EffectTickRate = 0f;         // 0이면 "한 번만 적용"

    public void ValidateAndClamp()
    {
        if (EffectValue < 0f) EffectValue = 0f;
        if (EffectDuration < 0f) EffectDuration = 0f;
        if (EffectTickRate < 0f) EffectTickRate = 0f;
        
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

    public string IconPath;
    private Sprite icon;
    public Sprite Icon
    {
        get
        {
            if (icon == null && !string.IsNullOrWhiteSpace(IconPath))
                icon = Resources.Load<Sprite>(IconPath);
            return icon;
        }
    }

    public ItemType Type = ItemType.Consumable;
    public EquipSlotFlags SlotFlags = EquipSlotFlags.General;  // 중복 슬롯 가능
    public float Weight = 0f;                                  // 1개당 무게
    public int StackLimit = 1;                                 // 중첩 가능 수

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
        
        SlotFlags |= EquipSlotFlags.General;
        
        Effect?.ValidateAndClamp();
    }

    public bool CanEquipTo(EquipSlotFlags slot) => (SlotFlags & slot) != 0;
}
