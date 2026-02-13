using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SessionInventory : IInventory
{
    private ItemStack[] items;

    public int MaxSlots => items?.Length ?? 0;
    public float MaxWeight { get; private set; }

    public InventoryType Type => InventoryType.InGame;
    public IReadOnlyList<ItemStack> Items => items;
    
    
    public event Action OnInventoryChanged;
    public event Action<int> OnSlotChanged;
    public float CurrentWeight => items.Sum(x => x.Data != null ? x.Data.Weight * x.amount : 0f);

    public int CurrentSlotCount => items.Count(x => x != null);


    public void SetupBackpack(int slots, float weightLimit)
    {
        var newItems = new ItemStack[slots];
        if (items != null)
        {
            int minSize = Mathf.Min(items.Length, slots);
            Array.Copy(items, newItems, minSize);
        }

        items = newItems;
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
        
        // 1. 기존에 겹칠 수 있는 스택이 있는지 먼저 확인
        for (int i = 0; i < items.Length; ++i)
        {
            if (items[i] != null && items[i].itemId == itemId && items[i].IsFull == false)
            {
                int canAdd = itemData.StackLimit - items[i].amount;
                int toAdd = Mathf.Min(canAdd, amount);
                
                items[i].amount += toAdd;
                amount -= toAdd;
                OnSlotChanged?.Invoke(i);

                if (amount <= 0)
                    break;
            }
        }
        
        // 2. 남은 수량이 있다면 빈 슬롯에 추가
        if (amount > 0)
        {
            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i] == null)
                {
                    int toAdd = Mathf.Min(amount, itemData.StackLimit);
                    items[i] = new ItemStack(itemId, toAdd);
                    amount -= toAdd;
                    OnSlotChanged?.Invoke(i);
                
                    if (amount <= 0)
                        break;
                }
            }
        }

        if (amount > 0)
        {
            Debug.LogWarning("[SessionInventory] ::: 슬롯 부족");
            OnInventoryChanged?.Invoke();
            return false;
        }
        
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void SetItemAt(int index, ItemStack stack)
    {
        if (index < 0 || index >= items.Length)
            return;

        items[index] = stack;
        OnSlotChanged?.Invoke(index);
        OnInventoryChanged?.Invoke();
    }

    public ItemStack GetItemAt(int index)
    {
        if (index < 0 || index >= items.Length)
            return null;

        return items[index];
    }

    public bool TryRemoveItem(string itemId, int amount)
    {
        if (GetTotalCount(itemId) < amount)
            return false;

        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (items[i] != null && items[i].itemId == itemId)
            {
                int toRemove = Mathf.Min(items[i].amount, amount);
                items[i].amount -= toRemove;
                amount -= toRemove;

                if (items[i].amount <= 0)
                    items[i] = null;

                OnSlotChanged?.Invoke(i);
                if (amount <= 0)
                    break;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        Array.Clear(items, 0, items.Length);
        OnInventoryChanged?.Invoke();
    }

    public int GetTotalCount(string itemId) => items.Where(x => x != null && x.itemId == itemId).Sum(x => x.amount);

    public void TransferToStorage(IInventory storage)
    {
        for (int i = 0; i < items.Length; ++i)
        {
            if (items[i] != null)
            {
                if (storage.AddItem(items[i].itemId, items[i].amount))
                {
                    items[i] = null;
                    OnSlotChanged?.Invoke(i);
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }
}