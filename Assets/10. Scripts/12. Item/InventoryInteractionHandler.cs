using UnityEngine;

public static class InventoryInteractionHandler
{
    public static ItemStack DraggedItem { get; private set; }
    public static IInventory SourceInventory { get; private set; }
    public static int SourceIndex { get; private set; }

    public static void BeginDrag(IInventory sourceInv, int index, CursorSlot cursorUI, Vector2 size)
    {
        DraggedItem = SourceInventory.GetItemAt(index);
        SourceInventory = sourceInv;
        SourceIndex = index;
        
        cursorUI.SetCursorItem(DraggedItem, size);
    }

    public static void Drop(ItemSlot targetSlot, CursorSlot cursorUI)
    {
        if (DraggedItem == null)
            return;

        IInventory targetInv = targetSlot.LinkedInventory;
        int targetIndex = targetSlot.SlotIndex;
        ItemStack targetItem = targetInv.GetItemAt(targetIndex);

        // 1. 동일 인벤토리 혹은 다른 인벤토리로의 이동/스왑
        if (targetInv.Type == InventoryType.Shop || SourceInventory.Type == InventoryType.Shop)
        {
            ExecuteTrade(SourceInventory, targetInv, DraggedItem);
            targetInv.SetItemAt(targetIndex, DraggedItem);
            DraggedItem = null;
        }
        else
        {
            targetInv.SetItemAt(targetIndex, DraggedItem);
            SourceInventory.SetItemAt(SourceIndex, targetItem);

            DraggedItem = null;
        }
        
        cursorUI.SetCursorItem(null, Vector2.zero);
    }

    public static void EndDrag(CursorSlot cursorUI)
    {
        if (DraggedItem != null)
        {
            SourceInventory.SetItemAt(SourceIndex, DraggedItem);
        }
        
        DraggedItem = null;
        SourceInventory = null;
        SourceIndex = -1;
        cursorUI.SetCursorItem(null, Vector2.zero);
    }
    

    private static void ExecuteTrade(IInventory source, IInventory target, ItemStack item)
    {
        // TODO : 여기서 상점 UI | 거래 매니저 통해서 가격 계산 및 골드 차감
        Debug.Log($"{item.itemId} 거래 시도");
    }
}