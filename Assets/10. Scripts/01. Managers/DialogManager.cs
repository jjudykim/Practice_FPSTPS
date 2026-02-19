using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public event Action<bool> OnDialogOpenChanged;
    
    [Header("Dialog Prefab")]
    [SerializeField] private GameObject defaultDialogPrefab;
    [SerializeField] private Transform dialogRoot;

    private DialogController activeDialog;
    
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

    private void OnDestroy()
    {
        RestoreCursorPolicy();
    }

    public void Open(NpcDialogInteractable npc, string startNodeId)
    {
        if (IsOpen)
            return;

        GameObject targetPrefab = (npc.CustomDialogPrefab != null) ? npc.CustomDialogPrefab : defaultDialogPrefab;
        
        if (targetPrefab == null)
        {
            Debug.LogError("[DialogManager] dialogPrefab is null.");
            return;
        }
        
        ApplyDialogCursorPolicy();
        
        GameObject go = Instantiate(targetPrefab, dialogRoot);
        activeDialog = go.GetComponent<DialogController>();

        if (activeDialog == null)
        {
            Debug.LogError("[DialogManager] dialogPrefab has no DialogController component.");
            Destroy(go);
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
        if (cursorOverrideApplied == false)
            return;

        Cursor.lockState = prevLockMode;
        Cursor.visible = prevCursorVisible;

        cursorOverrideApplied = false;
    }
}