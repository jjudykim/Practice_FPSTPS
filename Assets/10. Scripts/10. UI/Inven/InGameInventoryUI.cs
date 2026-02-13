using TMPro;
using UnityEngine;

public class InGameInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI weightText;

    private SessionInventory targetInventory;

    public void Initialize(SessionInventory inventory)
    {
        targetInventory = inventory;
        targetInventory.OnInventoryChanged += RefreshUI;
        
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (targetInventory != null)
            targetInventory.OnInventoryChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        // 1. 기존 슬롯 제거
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 2. 새로운 데이터로 슬롯 생성
        foreach (var itemStack in targetInventory.Items)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            //slot.GetComponent<ItemSlot>().SetData(itemStack);
        }
        
        // 3. 무게 정보 등 기타 UI 갱신
        amountText.text = $"{targetInventory.CurrentSlotCount} / {targetInventory.MaxSlots}";
        weightText.text = $"{targetInventory.CurrentWeight} / {targetInventory.MaxWeight}";
    }
}