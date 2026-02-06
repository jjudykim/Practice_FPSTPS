using System;

public enum ItemType
{
    Weapon,
    Equipment,
    Consumable
}

// 모든 아이템은 General을 기본으로 가짐
[Flags]
public enum EquipSlotFlags
{
    None      = 0,
    General   = 1 << 0,
    Head      = 1 << 1,
    Body      = 1 << 2,
    Back      = 1 << 3,
    QuickSlot = 1 << 4,
}

[Flags]
public enum EffectType
{
    None = 0,
    Heal = 1 << 0,
    Buff = 1 << 1,
    HealAndBuff = Heal | Buff
}