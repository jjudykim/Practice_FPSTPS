using System;
using System.Collections.Generic;

public interface IInventory
{
    IReadOnlyList<ItemStack> Items { get; }
    event Action OnInventoryChanged;
    
    bool AddItem(string itemId, int amount);
    bool TryRemoveItem(string itemId, int amount);
    void Clear();

    int GetTotalCount(string itemId);
}