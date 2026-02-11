using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [Header("Save Data")] 
    [SerializeField] private SaveDataManager saveDataManager;
    
    [Header("Groups")] 
    [SerializeField] private GameObject menuBtnGroup;
    [SerializeField] private GameObject saveLoadGroup;
    
    [SerializeField] private NameInputUI nameInputUI;
    
    [Header("Menu Buttons")]
    [SerializeField] private GameObject playButtonRoot;
    [SerializeField] private GameObject exitButtonRoot;
    [SerializeField] private GameObject backButtonRoot;

    [Header("Save Slots")]
    [SerializeField] private LobbySaveSlotView[] slotViews;
    
    [Header("Fade / Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private string gameSceneName = "TownScene";
    
    private bool isTransitioning;

    private void Awake()
    {
        saveDataManager = Managers.Instance.SaveData;
        
        SetMenuVisible(true);
        SetSaveLoadVisible(false);
        SetFade(0f);
    }

    private void Start()
    {
        BuildOrRefreshSlots();
    }
    
    // ===========================
    // public Methods
    // ===========================
    
    public void OnPlayPressed()
    {
        SetMenuVisible(false);
        SetSaveLoadVisible(true);

        BuildOrRefreshSlots();
    }

    public void OnBackPressed()
    {
        SetSaveLoadVisible(false);
        SetMenuVisible(true);
        
        isTransitioning = false;
        SetFade(0f);
    }

    public void OnExitPressed()
    {
        Application.Quit();
        
#if UNITY_EDITOR
        Debug.Log("[LobbyController] ::: Application.Quit() Called.");
#endif
    }

    public void OnSaveSlotPressed(int slotIndex)
    {
        if (isTransitioning)
            return;

        bool hasSave = saveDataManager.HasSaveFile(slotIndex);

        if (hasSave)
        {
            StartCoroutine(coFadeOutAndEnterGame(slotIndex, true));
        }
        else
        {
            nameInputUI.Open(slotIndex);
        }
        
    }

    public void OnSaveSlotPressed_0() => OnSaveSlotPressed(0);
    public void OnSaveSlotPressed_1() => OnSaveSlotPressed(1);
    public void OnSaveSlotPressed_2() => OnSaveSlotPressed(2);
    
    
    // ===========================
    // Slot Build / Refresh
    // ===========================

    private void BuildOrRefreshSlots()
    {
        for(int i = 0; i < slotViews.Length; ++i)
            RefreshSlotMeta(i);
    }

    private void RefreshSlotMeta(int slotIndex)
    {
        LobbySaveSlotView view = slotViews[slotIndex];
        if (view == null)
            return;

        if (saveDataManager.HasSaveFile(slotIndex) == false)
        {
            view.SetText("New Game", "", true);
            return;
        }

        SaveData data = saveDataManager.Load(slotIndex);
        if (data == null)
        {
            view.SetText("New Game", "", true);
            return;
        }

        string playerName = string.IsNullOrEmpty(data.progress.playerName) ? "Player" : data.progress.playerName;
        int level = data.progress.growth.level;
        
        string title = $"{playerName} / Lv. {level}"; 
        string meta = $"Last Save : {data.GetLastSaveLocalText()}";
        
        view.SetText(title, meta, true);
    }
    
    
    // =========================
    // Fade & Enter Flow
    // =========================
    private IEnumerator coFadeOutAndEnterGame(int slotIndex, bool hasSave)
    {
        isTransitioning = true;

        yield return CoFade(0f, 1f);

        if (hasSave)
        {
            saveDataManager.Load(slotIndex);
            Debug.Log($"[Lobby] Load Game (slot={slotIndex})");
        }
        else
        {
            saveDataManager.CreateNew(
                slotIndex,
                saveVersion: 1,
                saveImmediately: true
            );
            Debug.Log($"[Lobby] New Game (slot={slotIndex})");
        }

        Managers.Instance.Scene.LoadScene(gameSceneName);
    }

    private IEnumerator CoFade(float from, float to)
    {
        float t = 0f;
        fadeCanvasGroup.blocksRaycasts = true;

        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t / fadeOutDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }

    private void SetFade(float alpha)
    {
        fadeCanvasGroup.alpha = alpha;
        fadeCanvasGroup.blocksRaycasts = alpha > 0.001f;
    }

    // ===========================
    // Group Visible
    // ===========================
    private void SetMenuVisible(bool visible)
    {
        if (menuBtnGroup != null)
            menuBtnGroup.SetActive(visible);
    }

    private void SetSaveLoadVisible(bool visible)
    {
        if (saveLoadGroup != null)
            saveLoadGroup.SetActive(visible);
    }
}