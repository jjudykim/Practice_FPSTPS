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
    [SerializeField] private bool applyImmediatelyOnEnable = true;
    
    private void OnEnable()
    {
        EnsureCameraController();

        if (cameraController != null)
        {
            cameraController.OnModeChanged += HandleModeChanged;

            if (applyImmediatelyOnEnable)
            {
                HandleModeChanged(cameraController.Mode);
            }
        }
        else
        {
            Debug.LogWarning("[PlayerUIModeSwitcher] ::: CameraController를 찾을 수 없음");
        }
    }

    private void OnDisable()
    {
        if (cameraController != null)
            cameraController.OnModeChanged -= HandleModeChanged;
    }

    public void SetWorldUIRoot(GameObject worldUI)
    {
        worldUIRoot = worldUI;
    }

    private void EnsureCameraController()
    {
        if (cameraController == null)
        {
            cameraController = CameraController.Instance;

            if (cameraController == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraController = mainCam.GetComponent<CameraController>();
                }

                if (cameraController == null)
                {
                    cameraController = FindObjectOfType<CameraController>();
                }
            }
        }
    }

    public void SetUp(CameraController controller)
    {
        cameraController = controller;
        if (cameraController != null)
                HandleModeChanged(cameraController.Mode);
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

        if (worldUIComponents != null)
        {
            for (int i = 0; i < worldUIComponents.Length; i++)
            {
                if (worldUIComponents[i] != null)
                    worldUIComponents[i].ForceUpdateNow();
            }
        }
    }
}
