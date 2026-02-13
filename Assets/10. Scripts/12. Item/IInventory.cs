using System;
using System.Collections.Generic;

public enum InventoryType
{
    Storage,
    InGame,
    Shop
}

public interface IInventory
{
    InventoryType Type { get; }
    IReadOnlyList<ItemStack> Items { get; }
    int MaxSlots { get; }
    bool AddItem(string itemId, int amount);
    void SetItemAt(int index, ItemStack stack);
    ItemStack GetItemAt(int index);
    bool TryRemoveItem(string itemId, int amount);
    void Clear();

    int GetTotalCount(string itemId);
    
    event Action OnInventoryChanged;
}