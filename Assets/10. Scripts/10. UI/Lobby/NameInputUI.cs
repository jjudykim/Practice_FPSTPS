using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private string nextSceneName = "TownScene";

    private int pendingSlotIndex;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        gameObject.SetActive(false);
    }

    public void Open(int slotIndex)
    {
        pendingSlotIndex = slotIndex;
        gameObject.SetActive(true);

        if (nameInputField != null)
            nameInputField.text = "";
    }

    private void OnStartButtonClicked()
    {
        string inputName = nameInputField?.text.Trim();

        if (string.IsNullOrEmpty(inputName))
            return;

        var saveManager = Managers.Instance.SaveData;
        var newData = saveManager.CreateNew(pendingSlotIndex, 1, false);
        newData.progress.playerName = inputName;

        saveManager.Save();
        
        Managers.Instance.Scene.LoadScene(nextSceneName);
    }
}
