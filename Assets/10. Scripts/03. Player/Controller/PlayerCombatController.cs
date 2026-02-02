using System;
using System.Collections;
using jjudy;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombatController : MonoBehaviour
{
    private static readonly int IS_WEAPON_EQUIPPED = Animator.StringToHash("IsWeaponEquipped");
    private static readonly int IS_AIMING = Animator.StringToHash("IsAiming");
    
    private static readonly int TRIGGER_FIRE = Animator.StringToHash("Fire");
    private static readonly int TRIGGER_RELOAD = Animator.StringToHash("Reload");
    private static readonly int TRIGGER_EQUIP = Animator.StringToHash("Equip");
    private static readonly int TRIGGER_UNEQUIP = Animator.StringToHash("UnEquip");

    private static readonly int AIM_X = Animator.StringToHash("AimX");
    private static readonly int AIM_Y = Animator.StringToHash("AimY");

    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponRoot weaponRoot;
    [SerializeField] private WeaponBase weaponPrefabForTest;
    [SerializeField] private MonoBehaviour aimProviderSource;
    [SerializeField] private PlayerLookController lookController;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private CameraController cameraController;

    [Header("QuarterView Aim Facing")]
    [SerializeField] private float quarterAimTurnSpeed = 18f;     // 회전 보간 속도
    [SerializeField] private float quarterAimMaxDistance = 200f;  // 조준점 레이 최대 거리
    [SerializeField] private LayerMask quarterAimMask = ~0;
    [SerializeField] private bool rotateOnlyWhenAiming = true;   // true면 Aim 중에만 회전, false면 무기 장착 중이면 항상 회전

    [Header("UpperBody Layer Weights")]
    [Header("IK Weights")]
    [SerializeField, Range(0f, 1f)] private float ikWeightEquipped = 0.6f; // 장착 기본 IK
    [SerializeField, Range(0f, 1f)] private float ikWeightAiming = 1.0f;   // 조준 IK

    [SerializeField, Range(0f, 1f)] private float upperBodyWeightEquipped = 1.0f;
    [SerializeField, Range(0f, 1f)] private float upperBodyWeightAiming = 1.0f;
    [SerializeField] private string upperBodyLayerName = "UpperBody Layer";

    [Header("UI")]
    [SerializeField] private CrosshairUI crosshairUI;

    private IAimProvider aimProvider;
    private WeaponBase currentWeapon;

    public ObservableValue<float> ReloadElapsedObs { get; } = new ObservableValue<float>(0f);
    public ObservableValue<float> ReloadDurationObs { get; } = new ObservableValue<float>(1f);
    public ObservableValue<bool> ReloadVisibleObs { get; } = new ObservableValue<bool>(false);

    private bool wasReloading = false;
    private bool reloadCompleted = false;
    private float reloadCompleteTime = 0f;
    private bool reloadCooldownActive = false;

    public bool IsWeaponEquipped { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsReloading { get; private set; }

    public bool IsBusyByRoll => (moveController != null) && moveController.IsRolling;
    private bool IsBusyByRunForFire => (moveController != null) && moveController.IsRunning;
    
    public bool CanFire => IsWeaponEquipped
                           && (IsBusyByRoll == false)
                           && (IsBusyByRunForFire == false)
                           && (IsReloading == false);

    public bool CanAim => IsWeaponEquipped
                          && (IsBusyByRoll == false)
                          && (IsReloading == false);

    private InputManager input;
    private int upperBodyLayerIndex = -1;

    // Quarter aim point cache
    private int cachedAimFrame = -1;
    private bool cachedHasAimPoint = false;
    private Vector3 cachedAimPoint = Vector3.zero;

    private void OnEnable()
    {
        cameraController.OnModeChanged += HandleCameraModeChanged;
        RefreshCrosshairVisibility();
    }

    private void OnDisable()
    {
        cameraController.OnModeChanged -= HandleCameraModeChanged;
    }

    private void HandleCameraModeChanged(CameraController.CameraMode mode)
    {
        RefreshCrosshairVisibility();
        PushWeaponContext();
        RefreshRigWeights();
    }

    private void BindWeaponEvents(WeaponBase weapon)
    {
        if (weapon == null)
            return;

        weapon.OnShotFired += HandleShotFired;
    }

    private void UnbindWeaponEvents(WeaponBase weapon)
    {
        if (weapon == null)
            return;

        weapon.OnShotFired -= HandleShotFired;
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (cameraController == null)
            cameraController = Camera.main.GetComponent<CameraController>();

        input = Managers.Instance.Input;

        aimProvider = aimProviderSource as IAimProvider;
        if (aimProvider == null)
            Debug.LogError("[PlayerCombatCtrl] ::: aimProviderSource is not IAimProvider");

        if (weaponRoot == null)
            Debug.LogError("[PlayerCombatCtrl] ::: weaponRoot is null.");

        if (animator != null && string.IsNullOrEmpty(upperBodyLayerName) == false)
            upperBodyLayerIndex = animator.GetLayerIndex(upperBodyLayerName);

        ApplyAnimatorBools();
        ApplyAimDirectionParams();
        PushWeaponContext();
    }

    // TODO : 테스트용, 나중에 게임 매니저 붙이면 삭제
    private async void Start()
    {
        await Databases.Instance.PreloadAllAsync();
    }

    private void Update()
    {
        // ===========================
        // 0) 상태 갱신: Reload Lock (중앙 게이트)
        // ===========================
        IsReloading = (currentWeapon != null) && currentWeapon.IsReloading;
        UpdateReloadProgress(Time.deltaTime);

        bool rolling = IsBusyByRoll;

        // ===========================
        // 1) Equip Toggle
        // - Reload 중에는 Equip/Unequip도 무시 (패널티)
        // ===========================
        if (IsReloading == false)
        {
            if (GetEquipToggleDown())
                ToggleEquip();
        }
        
        if (IsWeaponEquipped == false)
        {
            if (IsAiming)
                SetAiming(false);

            return;
        }

        // ===========================
        // 2) Roll 상태 처리
        // ===========================
        if (rolling)
        {
            if (IsAiming)
                SetAiming(false);

            if (currentWeapon != null)
                currentWeapon.CancelReload();

            // CancelReload 이후 Reload 상태 재동기화
            IsReloading = (currentWeapon != null) && currentWeapon.IsReloading;

            ApplyAimDirectionParams();
            return;
        }

        // ===========================
        // 3) Reload Lock 구간
        // - Reload 중이면 Combat 입력을 전부 무시
        // ===========================
        if (IsReloading)
        {
            if (IsAiming)
                SetAiming(false);
            
            TickQuarterViewAimFacing();
            ApplyAimDirectionParams();
            return;
        }

        // ===========================
        // 4) 평상시 Combat 루프
        // ===========================
        TickAim();
        TickQuarterViewAimFacing();

        if (GetReloadDown())
            TryReload();

        TickFire();

        ApplyAimDirectionParams();
    }

    private void ToggleEquip()
    {
        IsWeaponEquipped = !IsWeaponEquipped;

        if (IsWeaponEquipped)
        {
            currentWeapon = weaponRoot.Equip(weaponPrefabForTest);
            
            if (currentWeapon == null)
            {
                IsWeaponEquipped = false;
                Debug.LogError("[PlayerCombatCtrl] Equip failed. currentWeapon is null.");
                ApplyAnimatorBools();
                RefreshCrosshairVisibility();
                RefreshRigWeights();
                return;
            }

            animator.SetTrigger(TRIGGER_EQUIP);
            BindWeaponEvents(currentWeapon);
            ResetReloadUI();
        }
        else
        {
            weaponRoot.Unequip();
            currentWeapon = null;
            animator.SetTrigger(TRIGGER_UNEQUIP);
            UnbindWeaponEvents(currentWeapon);
            ResetReloadUI();

            if (IsAiming)
                SetAiming(false);
        }

        ApplyAnimatorBools();
        PushWeaponContext();
        RefreshCrosshairVisibility();
        RefreshRigWeights();

        Debug.Log($"[PlayerCombatCtrl] ::: Equip = {IsWeaponEquipped}");
    }

    private void UpdateReloadProgress(float dt)
    {
        if (currentWeapon == null)
        {
            ResetReloadUI();
            wasReloading = false;
            reloadCooldownActive = false;
            return;
        }
        
        // 시작
        if (!wasReloading && IsReloading)
        {
            ReloadDurationObs.Value = Mathf.Max(0.01f, currentWeapon.Data.ReloadTime);
            ReloadElapsedObs.Value = 0f;
            ReloadVisibleObs.Value = true;
            reloadCompleted = false;
            reloadCooldownActive = false;
        }

        // 진행 중
        if (IsReloading)
        {
            ReloadElapsedObs.Value = Mathf.Min(ReloadElapsedObs.Value + dt, ReloadDurationObs.Value);

            if (!reloadCompleted && ReloadElapsedObs.Value >= ReloadDurationObs.Value)
            {
                reloadCompleted = true;
                reloadCompleteTime = Time.time;
            }
        }

        // 종료 감지
        if (wasReloading && !IsReloading)
        {
            if (reloadCompleted)
            {
                reloadCooldownActive = true;
                if (reloadCompleteTime <= 0f)
                    reloadCompleteTime = Time.time;
            }
            else
            {
                ResetReloadUI(); // 취소
            }
        }
        
        if (reloadCooldownActive)
        {
            if (Time.time - reloadCompleteTime >= 0.2f)
            {
                ResetReloadUI();
            }
        }

        wasReloading = IsReloading;
    }

    private void ResetReloadUI()
    {
        ReloadVisibleObs.Value = false;
        ReloadElapsedObs.Value = 0f;
        reloadCompleted = false;
        reloadCooldownActive = false;
    }

    private void TickAim()
    {
        if (CanAim == false)
        {
            if (IsAiming)
                SetAiming(false);
            return;
        }

        bool wantAim = GetAimHeld();
        if (wantAim != IsAiming)
            SetAiming(wantAim);
    }

    private void TickQuarterViewAimFacing()
    {
        if (cameraController == null)
            return;

        if (cameraController.Mode != CameraController.CameraMode.QuarterView)
            return;

        if (IsWeaponEquipped == false)
            return;

        if (rotateOnlyWhenAiming && IsAiming == false)
            return;

        if (IsBusyByRoll)
            return;

        if (TryGetQuarterAimPoint(out Vector3 aimPoint) == false)
            return;

        Vector3 origin = transform.position;
        if (aimProvider != null && aimProvider.Muzzle != null)
            origin = aimProvider.Muzzle.position;
        
        Debug.DrawLine(origin, aimPoint, Color.magenta, 0.1f);
        Debug.DrawRay(origin, transform.forward * 5f, Color.cyan, 0.1f);
        if (aimProvider != null && aimProvider.Muzzle != null)
            Debug.DrawRay(origin, aimProvider.Muzzle.forward * 5f, Color.red, 0.1f);

        
        Vector3 dir = aimPoint - origin;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        
        float t = 1f - Mathf.Exp(-quarterAimTurnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
    }

    private bool TryGetQuarterAimPoint(out Vector3 aimPoint)
    {
        if (cachedAimFrame == Time.frameCount)
        {
            aimPoint = cachedAimPoint;
            return cachedHasAimPoint;
        }

        cachedAimFrame = Time.frameCount;
        cachedHasAimPoint = false;
        cachedAimPoint = Vector3.zero;

        if (aimProvider != null)
        {
            if (aimProvider.TryGetAimPoint(quarterAimMaxDistance, quarterAimMask, out Vector3 hit))
            {
                cachedHasAimPoint = true;
                cachedAimPoint = hit;
                aimPoint = cachedAimPoint;
                return true;
            }
        }

        aimPoint = default;
        return false;
    }

    private void SetAiming(bool aiming)
    {
        IsAiming = aiming;

        ApplyAnimatorBools();
        PushWeaponContext();
        RefreshCrosshairVisibility();
        RefreshRigWeights();
        ApplyAimDirectionParams();

        Debug.Log($"[PlayerCombatCtrl] ::: Aiming = {IsAiming}");
    }

    private void PushWeaponContext()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.SetContext(new WeaponContext(aimProvider, transform, IsAiming));

        if (crosshairUI != null)
            crosshairUI.SetContext(currentWeapon.Data, IsAiming);
    }

    private void ApplyAimDirectionParams()
    {
        if (animator == null)
            return;

        // 1인칭
        if (cameraController != null && cameraController.Mode == CameraController.CameraMode.FirstPerson)
        {
            if (lookController == null)
                return;

            Vector2 look = lookController.LookLocalDir;
            animator.SetFloat(AIM_X, look.x);
            animator.SetFloat(AIM_Y, look.y);
            return;
        }

        // 쿼터뷰: 조준점 기준으로 AimX/AimY 세팅
        if (cameraController != null && cameraController.Mode == CameraController.CameraMode.QuarterView)
        {
            animator.SetFloat(AIM_X, 0f);
            animator.SetFloat(AIM_Y, 0f);
            return;
        }
    }

    private void HandleShotFired()
    {
        animator.SetTrigger(TRIGGER_FIRE);
    }

    private void TickFire()
    {
        if (CanFire == false)
            return;

        bool fireDown = input.FireDown;
        bool fireHeld = input.Fire;
        bool fireUp = input.FireUp;

        if (fireDown)
        {
            currentWeapon.TriggerDown();
            return;
        }

        if (fireHeld)
        {
            currentWeapon.TriggerHold();
            return;
        }

        if (fireUp)
            currentWeapon.TriggerUp();
    }

    private void TryReload()
    {
        if (IsReloading)
            return;

        if (IsBusyByRoll)
            return;

        if (IsWeaponEquipped == false)
            return;

        if (currentWeapon == null)
            return;

        if (IsAiming)
            SetAiming(false);

        animator.SetTrigger(TRIGGER_RELOAD);
        currentWeapon.Reload();

        Debug.Log("[PlayerCombatCtrl] ::: Reload Start");
    }

    private void ApplyAnimatorBools()
    {
        if (animator == null)
            return;

        animator.SetBool(IS_WEAPON_EQUIPPED, IsWeaponEquipped);
        animator.SetBool(IS_AIMING, IsAiming);
    }

    private void ApplyUpperBodyLayerWeight(float weight01)
    {
        if (animator == null)
            return;

        if (upperBodyLayerIndex < 0)
            return;

        float w = Mathf.Clamp01(weight01);
        animator.SetLayerWeight(upperBodyLayerIndex, w);
    }

    private bool GetEquipToggleDown() => input.OnWeapon;
    private bool GetReloadDown() => input.Reload;

    private bool GetAimHeld()
    {
        if (input.Aiming)
            return true;

        return input.Aiming;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        if (IsWeaponEquipped == false
            || currentWeapon == null
            || currentWeapon.AttachPoint == null)
        {
            SetIKWeightZero();
            return;
        }

        float w = currentWeapon.ikWeight;
        ApplyIK(AvatarIKGoal.LeftHand, currentWeapon.AttachPoint, w);

        void ApplyIK(AvatarIKGoal goal, Transform target, float ikWeight)
        {
            animator.SetIKPosition(goal, target.position);
            animator.SetIKPositionWeight(goal, ikWeight);

            animator.SetIKRotation(goal, target.rotation);
            animator.SetIKRotationWeight(goal, ikWeight);
        }

        void SetIKWeightZero()
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }

    void RefreshCrosshairVisibility()
    {
        if (crosshairUI == null)
            return;

        bool visible = false;
        bool followMouse = false;

        if (cameraController != null)
        {
            // 1인칭: 무기 장착 시 CrosshairUI 표시 (중앙 고정)
            // 쿼터뷰: 무기 장착 시 CrosshairUI 표시 (마우스 추적)
            if (IsWeaponEquipped)
            {
                if (cameraController.Mode == CameraController.CameraMode.FirstPerson)
                {
                    visible = true;
                    followMouse = false;
                }
                else if (cameraController.Mode == CameraController.CameraMode.QuarterView)
                {
                    visible = true;
                    followMouse = true;
                }
            }
            cameraController.SetQuarterViewCursorHidden(IsWeaponEquipped);
        }

        crosshairUI.gameObject.SetActive(visible);
        crosshairUI.SetFollowMouse(followMouse);
    }

    private void RefreshRigWeights()
    {
        if (IsWeaponEquipped == false)
        {
            ApplyUpperBodyLayerWeight(0f);
            return;
        }

        if (IsAiming)
            ApplyUpperBodyLayerWeight(upperBodyWeightAiming);
        else
            ApplyUpperBodyLayerWeight(upperBodyWeightEquipped);
    }
}
