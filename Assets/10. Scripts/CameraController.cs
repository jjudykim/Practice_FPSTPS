using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        QuarterView
    }

    [Header("Targets")] 
    [SerializeField] private PlayerLookController lookController;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headPivot;
    [SerializeField] private Transform firstPersonAnchor;

    [Header("Mode")] 
    [SerializeField] private CameraMode startMode = CameraMode.QuarterView;
    private CameraMode currentMode;
    public CameraMode Mode => currentMode;
    
    [Header("Smooting")]
    [SerializeField] private float positionSmooth = 18f;
    
    [Header("First Person Settings")]
    [SerializeField] private float firstPersonFov = 65f;
    [SerializeField] private bool lockCursorInFirstPerson = true;

    [Header("Quarter View Settings")] 
    [SerializeField] private float quarterFov = 55f;
    [SerializeField] private Vector3 fixedQuarterRot = new Vector3(45f, -45f, 0f);
    [SerializeField] private Vector3 quarterOffset = new Vector3(0f, 8f, -10f);

    [SerializeField] private bool rotateQuarterOffsetByPlayerYaw = false;
    [SerializeField] private bool lockQuarterRotationOnEnter = true;

    private Camera cam;
    private Quaternion fixedQuaterRotation;

    private void Awake()
    {
        cam = Camera.main;
        currentMode = startMode;

        ApplyModeImmediate(currentMode);
        fixedQuaterRotation = Quaternion.Euler(fixedQuarterRot);
    }

    private void LateUpdate()
    {
        if (Managers.Instance == null || Managers.Instance.Input == null)
            return;

        if (Managers.Instance.Input.ViewChange)
            ToggleMode();

        switch (currentMode)
        {
            case CameraMode.FirstPerson:
                TickFirstPerson();
                break;
            case CameraMode.QuarterView:
                TickQuaterView();
                break;
        }
    }
    
    private void TickFirstPerson()
    {
        if (lookController == null)
            return;
        
        float targetYaw = lookController.Yaw;
        float targetPitch = lookController.Pitch;
        
        if (firstPersonAnchor != null)
            transform.position = Vector3.Lerp(transform.position
                                            , firstPersonAnchor.position
                                            , Time.deltaTime * positionSmooth);
        
        Quaternion targetRot = Quaternion.Euler(targetPitch, targetYaw, 0f);
        
        transform.rotation = Quaternion.Slerp(transform.rotation
                                            , targetRot
                                            , Time.deltaTime * positionSmooth);
    }
    
    private void TickQuaterView()
    {
        if (playerRoot == null)
            return;

        Vector3 offset = quarterOffset;
        
        if (rotateQuarterOffsetByPlayerYaw)
            offset = Quaternion.Euler(0f, playerRoot.eulerAngles.y, 0f) * quarterOffset;
        
        Vector3 targetPos = playerRoot.position + offset;
        
        transform.position = Vector3.Lerp(transform.position
                                        , targetPos
                                        , Time.deltaTime * positionSmooth);
        
        if (lockQuarterRotationOnEnter)
            transform.rotation = fixedQuaterRotation;   
    }

    private void ToggleMode()
    {
        currentMode = (currentMode == CameraMode.FirstPerson) ? CameraMode.QuarterView :  CameraMode.FirstPerson;
        ApplyModeImmediate(currentMode);
    }
    
    private void ApplyModeImmediate(CameraMode mode)
    {
        if (cam != null)
            cam.fieldOfView = (mode == CameraMode.FirstPerson) ? firstPersonFov : quarterFov;

        if (lockCursorInFirstPerson == false)
            return;
        
        if (mode == CameraMode.FirstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public bool TryGetMouseWorldPointOnPlane(float planeY, out Vector3 worldPoint)
    {
        worldPoint = default;
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        if (plane.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 씬에서 목표 지점을 시각화(디버그용)
            if (playerRoot != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerRoot.position + quarterOffset, 0.2f);
            }
    
            if (firstPersonAnchor != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(firstPersonAnchor.position, 0.15f);
            }
        }
    #endif
}