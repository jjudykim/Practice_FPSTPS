using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private IInventory linkedInventory;
    private int slotIndex;
    private CursorSlot globalCursor;
    private RectTransform rectTransform;

    public int SlotIndex => slotIndex;
    public IInventory LinkedInventory => linkedInventory;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(IInventory inventory, int index, CursorSlot cursor)
    {
        linkedInventory = inventory;
        slotIndex = index;
        globalCursor = cursor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemStack stack = linkedInventory.GetItemAt(slotIndex);
        if (stack == null)
            return;

        Vector2 currentSize = rectTransform.rect.size;
        InventoryInteractionHandler.BeginDrag(linkedInventory, slotIndex, globalCursor, currentSize);
        
        linkedInventory.SetItemAt(slotIndex, null);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryInteractionHandler.DraggedItem != null)
            InventoryInteractionHandler.Drop(this, globalCursor);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryInteractionHandler.EndDrag(globalCursor);
    }
}