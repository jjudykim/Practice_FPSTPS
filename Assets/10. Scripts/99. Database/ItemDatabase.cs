using System;
using UnityEngine;

public class ItemDatabase : TSVDatabase<ItemData, ItemTsvRow>
{
    public ItemDatabase(string tableName = "Item", bool logOnLoad = true, StringComparer idComparer = null) 
        : base(tableName, logOnLoad, idComparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }

    protected override ItemData ConvertRowToData(ItemTsvRow row, int rowIndex)
    {
        ItemData d = new ItemData
        {
            Id = row.Id,
            DisplayName = row.DisplayName,
            Description = row.Description,
            IconPath = row.IconPath,
            Weight = row.Weight,
            StackLimit = row.StackLimit
        };

        // Enum Parsing: Type
        if (Enum.TryParse(row.Type, true, out ItemType type))
            d.Type = type;

        // Enum Parsing: SlotFlags
        if (Enum.TryParse(row.Slot, true, out EquipSlotFlags slot))
            d.SlotFlags = slot;

        // Effect Data
        d.Effect = new ItemEffectData
        {
            EffectValue = row.EffectValue,
            EffectDuration = row.EffectDuration,
            EffectTickRate = row.EffectSecRate
        };

        if (Enum.TryParse(row.EffectType, true, out EffectType effType))
            d.Effect.EffectType = effType;

        return d;
    }

    protected override void ValidateData(ItemData data, ItemTsvRow row, int rowIndex)
    {
        data.ValidateAndClamp();
    }
}