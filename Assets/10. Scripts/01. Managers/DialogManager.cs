using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public event Action<bool> OnDialogOpenChanged;
    
    [Header("Dialog Prefab")]
    [SerializeField] private GameObject dialogPrefab;
    [SerializeField] private Transform dialogRoot;

    private DialogController activeDialog;
    private NpcDialogInteractable activeNpcContext;
    
    public bool IsOpen => activeDialog != null;

    // ===========================
    // Cursor 상태 저장(원복용)
    // ===========================
    private CursorLockMode prevLockMode;
    private bool prevCursorVisible;
    private bool cursorOverrideApplied;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void Open(NpcDialogInteractable npc, string startNodeId)
    {
        if (IsOpen)
            return;

        if (dialogPrefab == null)
        {
            Debug.LogError("[DialogManager] dialogPrefab is null.");
            return;
        }
        activeNpcContext = npc;
        
        ApplyDialogCursorPolicy();
        
        GameObject go = Instantiate(dialogPrefab, dialogRoot);
        activeDialog = go.GetComponent<DialogController>();

        if (activeDialog == null)
        {
            Debug.LogError("[DialogManager] dialogPrefab has no DialogController component.");
            Destroy(go);
            activeNpcContext = null;
            
            RestoreCursorPolicy();
            return;
        }
        
        OnDialogOpenChanged?.Invoke(true);
        activeDialog.StartDialog(startNodeId);
    }

    public void Close()
    {
        if (IsOpen == false)
            return;
        
        Destroy(activeDialog.gameObject);
        activeDialog = null;
        
        OnDialogOpenChanged?.Invoke(false);
        
        RestoreCursorPolicy();
        
        HandleDialogFinished(activeNpcContext);
        activeNpcContext = null;
    }

    private void HandleDialogFinished(NpcDialogInteractable npc)
    {
        if (npc == null)
            return;

        if (npc.LoadSceneOnDialogFinish == false)
            return;

        if (string.IsNullOrWhiteSpace(npc.NextSceneName))
        {
            Debug.LogWarning("[DialogManager] NextSceneName is empty. Cannot load scene.");
            return;
        }
        
        SceneManager.LoadScene(npc.NextSceneName);
    }
    
    private void ApplyDialogCursorPolicy()
    {
        if (cursorOverrideApplied)
            return;

        prevLockMode = Cursor.lockState;
        prevCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cursorOverrideApplied = true;
    }

    private void RestoreCursorPolicy()
    {
        if (!cursorOverrideApplied)
            return;

        Cursor.lockState = prevLockMode;
        Cursor.visible = prevCursorVisible;

        cursorOverrideApplied = false;
    }
}