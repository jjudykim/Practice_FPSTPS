using UnityEngine;

public class PlayerUIModeSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;

    [Header("Roots to Toggle")]
    [SerializeField] private GameObject worldUIRoot; // 쿼터뷰에서 보여줄 월드 UI 루트(빌보드 포함)
    [SerializeField] private GameObject hudUIRoot;   // 1인칭에서 보여줄 HUD 루트(Canvas Screen Space)

    [Header("Optional: Force camera on WorldUI components")]
    [SerializeField] private WorldUI[] worldUIComponents; // worldUIRoot 아래의 WorldUI들(선택)

    [Header("Options")]
    [SerializeField] private bool applyImmediatelyOnStart = true;

    private void Awake()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
    }

    private void OnEnable()
    {
        if (cameraController != null)
            cameraController.OnModeChanged += HandleModeChanged;

        if (applyImmediatelyOnStart && cameraController != null)
            HandleModeChanged(cameraController.Mode);
    }

    private void OnDisable()
    {
        if (cameraController != null)
            cameraController.OnModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(CameraController.CameraMode mode)
    {
        // 토글 정책:
        // - QuarterView  : World UI ON, HUD OFF (원하면 둘 다 ON도 가능)
        // - FirstPersonView  : World UI OFF, HUD ON
        bool isFirst = (mode == CameraController.CameraMode.FirstPerson);

        if (worldUIRoot != null)
            worldUIRoot.SetActive(!isFirst);

        if (hudUIRoot != null)
            hudUIRoot.SetActive(isFirst);
        
        if (worldUIComponents != null && worldUIComponents.Length > 0)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                for (int i = 0; i < worldUIComponents.Length; i++)
                {
                    if (worldUIComponents[i] != null)
                        worldUIComponents[i].ForceUpdateNow();
                }
            }
        }
    }
}
