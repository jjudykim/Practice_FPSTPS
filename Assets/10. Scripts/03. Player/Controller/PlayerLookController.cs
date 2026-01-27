using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headPivot;
    
    [Header("Input Sensitivity")]
    [SerializeField] private float horizontalSensitivity = 1.0f;
    [SerializeField] private float verticalSensitivity = 1.0f;

    [Header("First Person Clamp (Offsets)")] 
    [SerializeField] private Vector2 firstPersonPitchMinMax = new Vector2(-40, 40f);
    
    [Header("Quarter Look")]
    [SerializeField] private float quarterHeadYawLimit = 60f;
    [SerializeField] private float quarterBodyFollowSpeed = 120f;
    [SerializeField] private float planeY = 0f;
    
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }
    public Vector2 LookLocalDir { get; private set; } = Vector2.up;
    public float LookDeltaYawDeg { get; private set; } = 0f;
    public Vector3 LookWorldDirFlat { get; private set; } = Vector3.forward;

    private float baseYaw;
    private float yawOffset;
    private float pitch;

    private InputManager input;

    private CameraController.CameraMode lastMode;

    private void Awake()
    {
        if (playerRoot == null)
            playerRoot = transform;
        
        input = Managers.Instance.Input;
        
        baseYaw = playerRoot.eulerAngles.y;
        yawOffset = 0f;
        pitch = 0f;

        Yaw = baseYaw;
        Pitch = pitch;

        lastMode = cameraController != null ? cameraController.Mode : CameraController.CameraMode.QuarterView;

        LookLocalDir = Vector2.up;
        LookDeltaYawDeg = 0f;
        LookWorldDirFlat = playerRoot.forward;
        if (LookWorldDirFlat.sqrMagnitude > 0.0001f)
            LookWorldDirFlat.Normalize();
    }

    private void Update()
    {
        if (cameraController.Mode != lastMode)
        {
            OnModeChanged(lastMode, cameraController.Mode);
            lastMode = cameraController.Mode;
        }

        TickInput();
    }

    private void LateUpdate()
    {
        if (cameraController == null || playerRoot == null)
            return;

        switch (cameraController.Mode)
        {
            case CameraController.CameraMode.FirstPerson:
                ApplyFirstPersonRotation();
                UpdateAimData_FirstPerson();
                break;
            case CameraController.CameraMode.QuarterView:
                ApplyQuaterRotation();
                break;
        }
    }

    private void TickInput()
    {
        if (input == null)
            return;

        float mouseX = input.POVX;
        float mouseY = input.POVY;

        if (cameraController.Mode == CameraController.CameraMode.FirstPerson)
        {
            yawOffset += mouseX * horizontalSensitivity;
            yawOffset = Mathf.Repeat(yawOffset + 180f, 360f) - 180f;

            pitch -= mouseY * verticalSensitivity;
            pitch = Mathf.Clamp(pitch, firstPersonPitchMinMax.x, firstPersonPitchMinMax.y);

            Yaw = baseYaw + yawOffset;
            Pitch = pitch;
        } 
    }
    
    private void ApplyFirstPersonRotation()
    {
        playerRoot.rotation = Quaternion.Euler(0f, Yaw, 0f);

        if (headPivot != null)
            headPivot.localRotation = Quaternion.Euler(0f, 0f, -Pitch);
    }

    private void ApplyQuaterRotation()
    {
        if (headPivot == null)
            return;

        if (cameraController.TryGetMouseWorldPointOnPlane(planeY, out Vector3 mouseWorld) == false)
        {
            Vector3 f = playerRoot.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.0001f)
                f.Normalize();

            LookWorldDirFlat = f;
            LookLocalDir = Vector2.up;
            LookDeltaYawDeg = 0f;
            return;
        }

        Vector3 toTarget = mouseWorld - playerRoot.position;
        toTarget.y = 0f;
        
        if (toTarget.sqrMagnitude < 0.0001f)
            return;
        
        LookWorldDirFlat = toTarget.normalized;

        float targetWorldYaw = Quaternion.LookRotation(toTarget, Vector3.up).eulerAngles.y;
        float bodyYaw = playerRoot.eulerAngles.y;

        float delta = Mathf.DeltaAngle(bodyYaw, targetWorldYaw);
        LookDeltaYawDeg = delta;

        float rad = delta * Mathf.Deg2Rad;
        LookLocalDir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        
 
        float clampedHeadYaw = Mathf.Clamp(delta, -quarterHeadYawLimit, quarterHeadYawLimit);
        headPivot.localRotation = Quaternion.Euler(-clampedHeadYaw, 0f, 0f);

        bool isClamped = Mathf.Abs(delta) > quarterHeadYawLimit + 0.5f;
        if (isClamped)
        {
            float step = Mathf.Sign(delta) * quarterBodyFollowSpeed * Time.deltaTime;
            float newYaw = bodyYaw + step;
            playerRoot.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }
    }
    
    private void UpdateAimData_FirstPerson()
    {
        LookDeltaYawDeg = 0f;
        LookLocalDir = Vector2.up;

        Vector3 fwd = playerRoot.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.0001f)
            fwd = Vector3.forward;
        fwd.Normalize();
        LookWorldDirFlat = fwd;
    }
    
    private void OnModeChanged(CameraController.CameraMode from, CameraController.CameraMode to)
    {
        if (to == CameraController.CameraMode.FirstPerson)
        {
            baseYaw = playerRoot.eulerAngles.y;
            yawOffset = 0f;
            Yaw = baseYaw;
            Pitch = pitch;
            
            UpdateAimData_FirstPerson();
        }
        else if (to == CameraController.CameraMode.QuarterView)
        {
            pitch = 0f;
            Pitch = pitch;

            if (headPivot != null)
                headPivot.localRotation = Quaternion.identity;
            
            LookLocalDir = Vector2.up;
            LookWorldDirFlat = playerRoot.forward;
            LookDeltaYawDeg = 0f;
            if (LookWorldDirFlat.sqrMagnitude > 0.0001f)
                LookWorldDirFlat.Normalize();
        }
    }
}