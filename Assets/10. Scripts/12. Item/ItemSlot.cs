using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Visuals")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject amountContainer;    // 수량 1개일 경우 수량 숨김 처리
    
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

    public void SetData(ItemStack stack)
    {
        if (stack == null || stack.Data == null)
        {
            ClearSlot();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = stack.Data.Icon;
            iconImage.enabled = true;
        }

        if (amountText != null)
        {
            amountText.text = stack.amount.ToString();
            
            if (amountContainer != null)
                amountContainer.SetActive(stack.amount > 1);
        }
    }

    private void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        
        if (amountContainer != null)
            amountContainer.SetActive(false);
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