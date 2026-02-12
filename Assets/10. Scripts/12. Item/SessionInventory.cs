using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SessionInventory : IInventory
{
    private List<ItemStack> items = new();

    public int MaxSlots { get; private set; }
    public float MaxWeight { get; private set; }

    public IReadOnlyList<ItemStack> Items => items;
    
    public event Action OnInventoryChanged;
    public float CurrentWeight => items.Sum(x => x.Data != null ? x.Data.Weight * x.amount : 0f);

    public int CurrentSlotCount => items.Count;
    
    public void SetupBackpack(int slots, float weightLimit)
    {
        MaxSlots = slots;
        MaxWeight = weightLimit;
        OnInventoryChanged?.Invoke();
    }
    
    public bool AddItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return false;
        
        var itemData = Databases.Instance.Item.GetOrNull(itemId);
        if (itemData == null)
            return false;

        float weightToAdd = itemData.Weight * amount;
        if (CurrentWeight + weightToAdd > MaxWeight)
        {
            Debug.LogWarning("무게 초과로 아이템을 주울 수 없음");
            return false;
        }

        foreach (var stack in items.Where(x => x.itemId == itemId && !x.IsFull))
        {
            int canAdd = itemData.StackLimit - stack.amount;
            int toAdd = Mathf.Min(canAdd, amount);

            stack.amount += toAdd;
            amount -= toAdd;

            if (amount <= 0)
                break;
        }

        while (amount > 0)
        {
            if (items.Count >= MaxSlots)
            {
                Debug.LogWarning("[SessionInventory] ::: 슬롯 부족 : 가방이 가득 찼습니다.");
                OnInventoryChanged?.Invoke();
                return false;
            }

            int toAdd = Mathf.Min(amount, itemData.StackLimit);
            items.Add(new ItemStack(itemId, toAdd));
            amount -= toAdd;
        }
        
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool TryRemoveItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return true;

        if (GetTotalCount(itemId) < amount)
            return false;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].itemId == itemId)
            {
                int toRemove = Mathf.Min(items[i].amount, amount);
                items[i].amount -= toRemove;
                amount -= toRemove;
                
                if (items[i].amount <= 0)
                    items.RemoveAt(i);

                if (amount <= 0)
                    break;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int GetTotalCount(string itemId) => items.Where(x => x.itemId == itemId).Sum(x => x.amount);

    public void TransferToStorage(StorageInventory storage)
    {
        foreach (var stack in items)
        {
            storage.AddItem(stack.itemId, stack.amount);
        }

        Clear();
    }
}