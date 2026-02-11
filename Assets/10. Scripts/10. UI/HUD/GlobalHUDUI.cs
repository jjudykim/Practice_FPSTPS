using System;
using jjudy;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalHUDUI : MonoBehaviour
{
    public static GlobalHUDUI Instance { get; private set; }

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    
    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI goldText;
    
    [Header("Growth")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Slider expBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameScene = scene.name != "LobbyScene" && scene.name != "MapScene";

        GetComponent<Canvas>().enabled = isGameScene;

        if (isGameScene)
            RefreshAll();
    }

    private void OnEnable()
    {
        if (Managers.Instance == null || Managers.Instance.SaveData == null)
            return;

        var saveData = Managers.Instance.SaveData;

        saveData.Gold.OnValueChanged += OnGoldChanged;
        saveData.Level.OnValueChanged += OnLevelChanged;
        saveData.Exp.OnValueChanged += OnExpChanged;
        saveData.MaxExp.OnValueChanged += OnMaxExpChanged;
        
        RefreshAll();
    }

    private void OnDisable()
    {
        if (Managers.Instance == null || Managers.Instance.SaveData == null)
            return;
        
        var saveData = Managers.Instance.SaveData;
        
        saveData.Gold.OnValueChanged -= OnGoldChanged;
        saveData.Level.OnValueChanged -= OnLevelChanged;
        saveData.Exp.OnValueChanged -= OnExpChanged;
        saveData.MaxExp.OnValueChanged -= OnMaxExpChanged;
    }

    public void RefreshAll()
    {
        var saveData = Managers.Instance.SaveData;
        if (saveData.Data == null)
            return;

        if (nameText != null)
            nameText.text = saveData.Data.progress.playerName;
        
        UpdateGoldUI(saveData.Gold.Value);
        UpdateLevelUI(saveData.Level.Value);
        UpdateExpUI(saveData.Exp.Value, saveData.MaxExp.Value);
    }
    
    
    // =========================================
    //              Event Handlers
    // =========================================
    private void OnGoldChanged(int prev, int curr) => UpdateGoldUI(curr);
    private void OnLevelChanged(int prev, int curr) => UpdateLevelUI(curr);
    private void OnExpChanged(int prev, int curr) => UpdateExpUI(curr, Managers.Instance.SaveData.MaxExp.Value);
    private void OnMaxExpChanged(int prev, int curr) => UpdateExpUI(Managers.Instance.SaveData.Exp.Value, curr);

    
    // =========================================
    //              UI Update Logic
    // =========================================
    private void UpdateGoldUI(int amount)
    {
        if (goldText != null)
            goldText.text = amount.ToString("N0");  // 천 단위 콤마 추가
    }

    private void UpdateLevelUI(int level)
    {
        if (levelText != null)
            levelText.text = $"LV. {level}";
    }

    private void UpdateExpUI(int current, int max)
    {
        if (expText != null)
            expText.text = $"{current} / {max}";
        
        if (expBar != null)
            expBar.value = max > 0 ? (float)current / max : 0f;
    }
}
