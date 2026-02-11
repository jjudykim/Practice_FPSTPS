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
        var player = Player.Instance;
        if (player == null)
            return;
        
        player.SetPositionAndRotation(entryPoint.position, entryPoint.rotation, entryPoint.localScale);
        player.ResetForTown();
        
        ApplyTownCameraAndCursorPolicy();
    }

    private void OnDisable()
    {
        if (cursorPolicyApplied)
        {
            Cursor.lockState = prevLockMode;
            Cursor.visible = prevCursorVisible;
            cursorPolicyApplied = false;
        }
    }

    private async void Start()
    {
        await Databases.Instance.PreloadAllAsync();
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

        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetMode(CameraController.CameraMode.FirstPerson);
        }
    }
}
