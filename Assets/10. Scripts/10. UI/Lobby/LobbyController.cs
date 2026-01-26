using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("Groups")] 
    [SerializeField] private GameObject menuBtnGroup;
    [SerializeField] private GameObject saveLoadGroup;
    
    [Header("Menu Buttons")]
    [SerializeField] private GameObject playButtonRoot;
    [SerializeField] private GameObject exitButtonRoot;
    [SerializeField] private GameObject backButtonRoot;

    [Header("Save Slot Buttons")] 
    [SerializeField] private LobbySaveSlotView saveSlotPrefab;
    [SerializeField] private Transform saveSlotParent;
    [SerializeField] private int slotCount = 3;
    
    [Header("Fade UI / Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private string gameSceneName = "TownScene";

    private LobbySaveSlotView[] slotViews;

    private void Awake()
    {
        SetMenuVisible(true);
        SetSaveLoadVisible(false);

        SetFade(0f);

        TryBindClickEvent(playButtonRoot, OnPlayPressed);
        TryBindClickEvent(exitButtonRoot, OnExitPressed);
        TryBindClickEvent(backButtonRoot, OnBackPressed);
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
        bool hasSave = HasSavedGame(slotIndex);

        if (hasSave)
            StartCoroutine(coFadeOutAndEnterGame(slotIndex, isNewGame: false));
        else
            StartCoroutine(coFadeOutAndEnterGame(slotIndex, isNewGame: true));
    }

    public void OnSaveSlotPressed_0() => OnSaveSlotPressed(0);
    public void OnSaveSlotPressed_1() => OnSaveSlotPressed(1);
    public void OnSaveSlotPressed_2() => OnSaveSlotPressed(2);
    public void OnSaveSlotPressed_3() => OnSaveSlotPressed(3);
    
    
    // ===========================
    // Slot Build / Refresh
    // ===========================

    private void BuildOrRefreshSlots()
    {
        if (saveSlotPrefab == null)
            return;

        if (saveSlotParent == null)
            return;

        if (slotViews == null || slotViews.Length != slotCount)
        {
            for (int i = saveSlotParent.childCount - 1; i >= 0; i--)
            {
                Destroy(saveSlotParent.GetChild(i).gameObject);
            }

            slotViews = new LobbySaveSlotView[slotCount];

            for (int i = 0; i < slotCount; ++i)
            {
                LobbySaveSlotView view = Instantiate(saveSlotPrefab, saveSlotParent);
                view.name = $"SaveSlotView_{i + 1}";

                int captured = i;
                view.SetSlotIndex(captured);
                
                view.SetOnClick(() => OnSaveSlotPressed(captured));
                TryBindClickEvent(view.gameObject, () => OnSaveSlotPressed(captured));

                slotViews[i] = view;
            }
        }

        for (int i = 0; i < slotViews.Length; ++i)
        {
            RefreshSlotMeta(i);
            Debug.Log($"Refresh Done : {i}Slot");
        }
    }

    private void RefreshSlotMeta(int slotIndex)
    {
        if (slotViews == null || slotIndex < 0 || slotIndex >= slotViews.Length)
            return;

        LobbySaveSlotView view = slotViews[slotIndex];
        if (view == null)
            return;

        bool hasSave = HasSavedGame(slotIndex);

        string title;
        string meta;

        if (hasSave == false)
        {
            title = "New Game";
            meta = string.Empty;
            
            view.SetText(title, meta, false);
            return;
        }
        
        SaveMeta metaData = GetSavedMeta(slotIndex);
        title = $"Stage {metaData.clearedStage}";
        meta = metaData.lastSavedDateTimeString;
        
        view.SetText(title, meta, true);
    }
    
    
    // =========================
    // Fade & Enter Flow
    // =========================
    private IEnumerator coFadeOutAndEnterGame(int slotIndex, bool isNewGame)
    {
        yield return CoFade(0f, 1f, fadeOutDuration);
        
        // TODO : 추후 실제 저장/로드로 변경
        if (isNewGame)
            Debug.Log($"[LobbyController] >>> New Game Start (slot={slotIndex})");
        else
            Debug.Log($"[LobbyController] >>> Load Game (slot={slotIndex})");

        if (string.IsNullOrEmpty(gameSceneName) == false)
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log("[LobbyController] gameSceneName is Empty.");
        }
    }

    private IEnumerator CoFade(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("[LobbyController] fadeCanvasGroup is null");
            yield break;
        }

        float t = 0f;
        SetFade(from);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = (duration <= 0f) ? 1f : Mathf.Clamp01(t / duration);
            SetFade(Mathf.Lerp(from, to, a));
            yield return null;
        }

        SetFade(to);
    }

    private void SetFade(float alpha)
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = alpha;

        bool block = alpha > 0.001f;
        fadeCanvasGroup.blocksRaycasts = block;
        fadeCanvasGroup.interactable = block;
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
    
    
    // ===========================
    // Save Data
    // ===========================
    
    [Serializable]
    private struct SaveMeta
    {
        public string lastSavedDateTimeString; // 예: "2026-01-26 04:12"
        public int clearedStage;               // 예: 12
    }

    private bool HasSavedGame(int slotIndex)
    {
        return PlayerPrefs.GetInt(GetSaveExistsKey(slotIndex), 0) == 1;
    }

    private SaveMeta GetSavedMeta(int slotIndex)
    {
        SaveMeta meta = new SaveMeta();
        meta.lastSavedDateTimeString = PlayerPrefs.GetString(GetSaveDateKey(slotIndex), "Unknown Date");
        meta.clearedStage = PlayerPrefs.GetInt(GetSaveStageKey(slotIndex), 0);
        return meta;
    }

    private string GetSaveExistsKey(int slotIndex) => $"SAVE_SLOT_{slotIndex}_EXISTS";
    private string GetSaveDateKey(int slotIndex) => $"SAVE_SLOT_{slotIndex}_LAST_DATE";
    private string GetSaveStageKey(int slotIndex) => $"SAVE_SLOT_{slotIndex}_STAGE";

    // 테스트용 컨텍스트 메뉴
    [ContextMenu("TEST: Mark Slot1 Has Data (date/stage)")]
    private void TestMarkSlot1()
    {
        int slotIndex = 0;
        PlayerPrefs.SetInt(GetSaveExistsKey(slotIndex), 1);
        PlayerPrefs.SetString(GetSaveDateKey(slotIndex), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        PlayerPrefs.SetInt(GetSaveStageKey(slotIndex), UnityEngine.Random.Range(1, 30));
        PlayerPrefs.Save();

        Debug.Log("[LobbyController] TEST: Slot1 데이터를 임시로 생성했습니다.");
        BuildOrRefreshSlots();
    }

    [ContextMenu("TEST: Clear Slot1 Data")]
    private void TestClearSlot1()
    {
        int slotIndex = 0;
        PlayerPrefs.DeleteKey(GetSaveExistsKey(slotIndex));
        PlayerPrefs.DeleteKey(GetSaveDateKey(slotIndex));
        PlayerPrefs.DeleteKey(GetSaveStageKey(slotIndex));
        PlayerPrefs.Save();

        Debug.Log("[LobbyController] TEST: Slot1 데이터를 삭제했습니다.");
        BuildOrRefreshSlots();
    }

    // ===========================
    // Modern UI Pack 대응: 클릭 이벤트 자동 바인딩(리플렉션)
    // ===========================

    private void TryBindClickEvent(GameObject root, UnityAction callback)
    {
        if (root == null || callback == null)
            return;

        // 1) Unity UI Button이 있으면 우선 처리(혼용 가능)
        var unityButtons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        if (unityButtons != null && unityButtons.Length > 0)
        {
            foreach (var b in unityButtons)
            {
                if (b == null) continue;
                b.onClick.RemoveListener(callback);
                b.onClick.AddListener(callback);
            }

            Debug.Log($"[LobbyController] Bound via Unity Button : {root.name} (count={unityButtons.Length})");
            return;
        }

        // 2) UnityEvent 필드/프로퍼티 중 클릭 계열 이름을 가진 것을 찾아 바인딩
        var components = root.GetComponentsInChildren<Component>(true);
        foreach (var comp in components)
        {
            if (comp == null) continue;

            Type type = comp.GetType();

            // Fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (!typeof(UnityEvent).IsAssignableFrom(f.FieldType))
                    continue;

                string lowerName = f.Name.ToLowerInvariant();
                if (!IsClickLikeEventName(lowerName))
                    continue;

                UnityEvent evt = f.GetValue(comp) as UnityEvent;
                if (evt == null) continue;

                evt.RemoveListener(callback);
                evt.AddListener(callback);

                Debug.Log($"[LobbyController] Bound via Reflection Field UnityEvent : {type.Name}.{f.Name} on {root.name}");
                return;
            }

            // Properties
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (!typeof(UnityEvent).IsAssignableFrom(p.PropertyType))
                    continue;

                if (!p.CanRead) continue;

                string lowerName = p.Name.ToLowerInvariant();
                if (!IsClickLikeEventName(lowerName))
                    continue;

                UnityEvent evt = null;
                try { evt = p.GetValue(comp, null) as UnityEvent; }
                catch { /* 안전 처리 */ }

                if (evt == null) continue;

                evt.RemoveListener(callback);
                evt.AddListener(callback);

                Debug.Log($"[LobbyController] Bound via Reflection Property UnityEvent : {type.Name}.{p.Name} on {root.name}");
                return;
            }
        }

        Debug.LogWarning($"[LobbyController] Auto bind failed on '{root.name}'. Modern UI Pack 이벤트에서 LobbyController public 메서드로 수동 연결해주세요.");
    }

    private bool IsClickLikeEventName(string lowerName)
    {
        return lowerName.Contains("click")
               || lowerName.Contains("press")
               || lowerName.Contains("submit")
               || lowerName.Contains("select")
               || lowerName.Contains("pointer")
               || lowerName.Contains("confirm");
    }
}