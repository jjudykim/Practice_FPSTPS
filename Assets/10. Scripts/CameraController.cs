using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        QuarterView
    }

    public event Action<CameraMode> OnModeChanged;

    [Header("Targets")] 
    [SerializeField] private PlayerLookController lookController;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headPivot;
    [SerializeField] private Transform firstPersonAnchor;

    [Header("Mode")] 
    [SerializeField] private CameraMode startMode = CameraMode.QuarterView;
    private CameraMode currentMode;
    public CameraMode Mode => currentMode;
    
    [Header("Cursor")]
    [SerializeField] private Texture2D normalCursor;
    
    [Header("Smooting")]
    [SerializeField] private float positionSmooth = 18f;
    
    [Header("First Person Settings")]
    [SerializeField] private float firstPersonFov = 65f;
    [SerializeField] private bool lockCursorInFirstPerson = true;
    [SerializeField] private float aimForwardOffset = 0.06f;
    [SerializeField] private float aimFirstPersonFov = 50f;

    [Header("Quarter View Settings")] 
    [SerializeField] private float quarterFov = 55f;
    [SerializeField] private Vector3 fixedQuarterRot = new Vector3(45f, -45f, 0f);
    [SerializeField] private Vector3 quarterOffset = new Vector3(0f, 8f, -10f);

    [SerializeField] private bool rotateQuarterOffsetByPlayerYaw = false;
    [SerializeField] private bool lockQuarterRotationOnEnter = true;

    private Camera cam;
    private Quaternion fixedQuaterRotation;
    private bool isAiming = false;
    private bool hideCursorInQuarterView = false;

    private void Awake()
    {
        cam = Camera.main;
        currentMode = startMode;
        
        fixedQuaterRotation = Quaternion.Euler(fixedQuarterRot);

        ApplyModeImmediate(currentMode);
        OnModeChanged?.Invoke(currentMode);
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

    public void SetQuarterViewCursorHidden(bool hidden)
    {
        hideCursorInQuarterView = hidden;
        
        if (currentMode == CameraMode.QuarterView)
            ApplyModeImmediate(currentMode);
    }

    private void TickFirstPerson()
    {
        if (lookController == null)
            return;
        
        float targetYaw = lookController.Yaw;
        float targetPitch = lookController.Pitch;

        Quaternion targetRot = Quaternion.Euler(targetPitch, targetYaw, 0f);
        
        if (firstPersonAnchor != null)
        {
            Vector3 targetPos = firstPersonAnchor.position;

            if (isAiming)
            {
                Vector3 forward = targetRot * Vector3.forward;
                targetPos += forward * aimForwardOffset;
            }
            
            transform.position = Vector3.Lerp(transform.position, targetPos , Time.deltaTime * positionSmooth);
        }
        
        transform.rotation = Quaternion.Slerp(transform.rotation , targetRot, Time.deltaTime * positionSmooth);
        
        if (cam != null)
        {
            float targetFov = isAiming ? aimFirstPersonFov : firstPersonFov;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * positionSmooth);
        }
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

        if (currentMode == CameraMode.FirstPerson)
            isAiming = false;
        
        OnModeChanged?.Invoke(currentMode);
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
            bool showCursor = !hideCursorInQuarterView;
            Cursor.visible = showCursor;
            
            if (showCursor)
                Cursor.SetCursor(normalCursor, new Vector2(0, 0), CursorMode.Auto);
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