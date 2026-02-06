using UnityEngine;

/// <summary>
/// Town Scene 전용 규칙 적용 Bootstrap
/// - Town에서는 전투(무기/Aim/Fire)를 막는다.
/// - 전투 씬으로 넘어갈 때는 원복한다.
/// 
/// 설계 의도:
/// - DontDestroyOnLoad Player를 "삭제/추가"하지 않는다.
/// - enable/disable 및 장착 해제 같은 '정책 적용'만 담당한다.
/// </summary>
public class TownBootstrap : MonoBehaviour
{
    [Header("Target Player (Dont Destroy Singleton)")]
    [SerializeField] private GameObject playerRoot; // PlayerCat 루트
    [SerializeField] private Transform entryPoint;
    
    [Header("Disable Objects")]
    [SerializeField] private GameObject combatRoot;

    [Header("Disable Components")]
    [SerializeField] private PlayerCombatController playerCombatController;
    
    [Header("Town Camera Policy")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private bool forceFirstPerson = true;
    [SerializeField] private bool showCursorInTown = true;

    // ===========================
    // 내부 상태 저장(원복용)
    // ===========================
    private bool prevCombatRootEnabled;
    private bool prevCombatEnabled;

    private bool applied;
    
    // Cursor 원복용
    private CursorLockMode prevLockMode;
    private bool prevCursorVisible;
    private bool cursorPolicyApplied;

    private void OnEnable()
    {
        ApplyTownRules();
        playerRoot.transform.position = entryPoint.position;
        playerRoot.transform.rotation = entryPoint.rotation;
    }

    private void OnDisable()
    {
        RestoreRules();
    }

    private async void Start()
    {
        await Databases.Instance.PreloadAllAsync();
    }

    /// <summary>
    /// Town 씬 규칙 적용
    /// </summary>
    public void ApplyTownRules()
    {
        if (applied)
            return;

        if (playerRoot == null)
        {
            Debug.LogError("[TownBootstrap] playerRoot is null. Assign PlayerCat root in Inspector.");
            return;
        }

        // ---------------------------
        // 현재 상태 저장
        // ---------------------------
        prevCombatRootEnabled = combatRoot != null && combatRoot.activeSelf;
        prevCombatEnabled = playerCombatController != null && playerCombatController.enabled;

        // ---------------------------
        // Town 규칙 적용
        // ---------------------------
        // 전투 컨트롤러 차단 (가장 강력한 1차 게이트)
        if (playerCombatController != null)
            playerCombatController.enabled = false;
        
        // 전투관련 요소 차단 (가장 강력한 1차 게이트)
        if (combatRoot != null)
            combatRoot.SetActive(false);
        
        ApplyTownCameraAndCursorPolicy();

        applied = true;

        Debug.Log("[TownBootstrap] Town rules applied.");
    }

    /// <summary>
    /// Town 규칙 원복 (전투 씬으로 넘어갈 때 복원)
    /// </summary>
    public void RestoreRules()
    {
        if (applied == false)
            return;

        // 저장했던 상태로 되돌림
        if (playerCombatController != null)
            playerCombatController.enabled = prevCombatEnabled;

        if (combatRoot != null)
            combatRoot.SetActive(prevCombatRootEnabled);

        RestoreTownCameraAndCursorPolicy();
            
        applied = false;

        Debug.Log("[TownBootstrap] Town rules restored.");
    }
    
    private void ApplyTownCameraAndCursorPolicy()
    {
        // 카메라는 인스펙터에서 StartMode를 FirstView로 설정해놨음
        
        if (showCursorInTown)
        {
            if (!cursorPolicyApplied)
            {
                prevLockMode = Cursor.lockState;
                prevCursorVisible = Cursor.visible;
                cursorPolicyApplied = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    private void RestoreTownCameraAndCursorPolicy()
    {
        if (cursorPolicyApplied == false)
            return;

        Cursor.lockState = prevLockMode;
        Cursor.visible = prevCursorVisible;
        cursorPolicyApplied = false;
    }
}
