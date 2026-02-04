using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;     // 전체 UI
    [SerializeField] private TMP_Text promptText;

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show(string message)
    {
        if (root != null)
            root.SetActive(true);

        if (promptText != null)
            promptText.SetText(message);
    }
}