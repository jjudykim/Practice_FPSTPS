using TMPro;
using UnityEngine;

public class InGameInventoryUI : MonoBehaviour
{
    [SerializeField] private CursorSlot cursorSlot;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI weightText;

    private SessionInventory targetInventory;

    public void Initialize(SessionInventory inventory)
    {
        if (targetInventory != null)
            targetInventory.OnInventoryChanged -= RefreshUI;
        
        targetInventory = inventory;
        targetInventory.OnInventoryChanged += RefreshUI;
        
        RefreshUI();
        
        Debug.Log("[InGameInventory] ::: 인벤토리 UI 초기화 완료");
    }

    private void OnDestroy()
    {
        if (targetInventory != null)
            targetInventory.OnInventoryChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        // 1. 기존 슬롯 제거
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 2. 새로운 데이터로 슬롯 생성
        for(int i = 0; i < targetInventory.MaxSlots; ++i)
        {
            var slotGo = Instantiate(slotPrefab, slotContainer);
            var slotScript = slotGo.GetComponent<ItemSlot>();

            if (slotScript != null)
            {
                slotScript.Setup(targetInventory, i, cursorSlot);
                var itemStack = targetInventory.GetItemAt(i);
                slotScript.SetData(itemStack);
            }
        }
        
        // 3. 무게 정보 등 기타 UI 갱신
        amountText.text = $"{targetInventory.CurrentSlotCount} / {targetInventory.MaxSlots}";
        weightText.text = $"{targetInventory.CurrentWeight} / {targetInventory.MaxWeight}";
    }
}