using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorSlot : MonoBehaviour
{
    [Header("UI Components")] 
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Settings")] 
    [SerializeField] private Vector2 offset = new Vector2(0f, 0f);

    private RectTransform rectTransform;
    public ItemStack HeldStack { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (HeldStack != null)
        {
            transform.position = (Vector2)Input.mousePosition + offset;
        }
    }

    public void SetCursorItem(ItemStack stack, Vector2 size)
    {
        HeldStack = stack;

        if (stack == null || stack.Data == null)
        {
            Hide();
            return;
        }

        rectTransform.sizeDelta = size;
        Show(stack);
    }

    private void Show(ItemStack stack)
    {
        gameObject.SetActive(true);

        if (stack.Data.Icon != null)
        {
            itemIcon.sprite = stack.Data.Icon;
            itemIcon.gameObject.SetActive(true);
        }
        else
        {
            itemIcon.gameObject.SetActive(false);
        }

        amountText.text = stack.amount > 1 ? stack.amount.ToString() : string.Empty;
    }

    private void Hide()
    {
        HeldStack = null;
        gameObject.SetActive(false);
    }
}