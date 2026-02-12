using System;

[Serializable]
public class ItemStack
{
    public string itemId;
    public int amount;

    public ItemData Data => Databases.Instance.Item.GetOrNull(itemId);
    
    public ItemStack() { }
    
    public ItemStack(string id, int amount = 1)
    {
        itemId = id;
        this.amount = amount;
    }
    
    public bool IsFull => Data != null && amount >= Data.StackLimit;
}