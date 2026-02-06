using System;

public class ItemDatabase : TSVDatabase<ItemData, ItemTsvRow>
{
    public ItemDatabase(string tableName = "Item", bool logOnLoad = true, StringComparer idComparer = null) 
        : base(tableName, logOnLoad, StringComparer.OrdinalIgnoreCase)
    {
    }

    protected override ItemData ConvertRowToData(ItemTsvRow row, int rowIndex)
    {
        ItemData d = new ItemData();

        return d;
    }
}