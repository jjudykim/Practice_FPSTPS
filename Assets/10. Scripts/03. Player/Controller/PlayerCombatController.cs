using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("New Weapon System")]
    [SerializeField] private PlayerWeaponInventory inventory;
    [SerializeField] private WeaponPrefabRegistry prefabRegistry;
    [SerializeField] private int currentSlotIndex = 0;
    [SerializeField] private WeaponBase currentWeapon = null;
    
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponRoot weaponRoot;
    [SerializeField] private MonoBehaviour aimProviderSource;
    [SerializeField] private PlayerLookController lookController;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private AimLineController aimLineController;
    
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

    [Header("UI Bindings")]
    [SerializeField] private CrosshairUI crosshairUI;
    [SerializeField] private WeaponMagazineUIBinder magazineBinder;
    [SerializeField] private GameObject worldUIRoot;
    [SerializeField] private GameObject uiBinder;
    
    
    private IAimProvider aimProvider;
    
    private WeaponContext weaponContext;
    private InputManager input;
    
    // 무기별 잔여 탄약 저장소 (ID 기반)
    private readonly Dictionary<string, int> savedAmmoByWeaponKey = new Dictionary<string, int>();
    
    public ObservableValue<float> ReloadElapsedObs { get; } = new ObservableValue<float>(0f);
    public ObservableValue<float> ReloadDurationObs { get; } = new ObservableValue<float>(1f);
    public ObservableValue<bool> ReloadVisibleObs { get; } = new ObservableValue<bool>(false);

    private bool wasReloading = false;
    private bool reloadCompleted = false;
    private float reloadCompleteTime = 0f;
    private bool reloadCooldownActive = false;

    // 상태 프로퍼티
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
    
    private int upperBodyLayerIndex = -1;

    public GameObject WorldUIRoot => worldUIRoot;
    public GameObject UIBinder => uiBinder;

    public void SetCrossHairUI(CrosshairUI ui)
    {
        crosshairUI = ui;
    }

    // Quarter aim point cache
    private int cachedAimFrame = -1;
    private bool cachedHasAimPoint = false;
    private Vector3 cachedAimPoint = Vector3.zero;
    
    private void OnEnable()
    {
        cameraController.OnModeChanged += HandleCameraModeChanged;
        BindAimProviderSafe();
        PushWeaponContext();
        RefreshCrosshairVisibility();
    }

    private void OnDisable()
    {
        cameraController.OnModeChanged -= HandleCameraModeChanged;
    }

    private void Start()
    {
        InitializeCombatState();
    }

    public void InitializeCombatState()
    {
        // 1. 물리적 무기 모델 제거
        if (weaponRoot != null)
            weaponRoot.Unequip();
        
        // 2. 참조 초기화
        currentWeapon = null;
        currentSlotIndex = -1;
        IsWeaponEquipped = false;
        IsAiming = false;
        IsReloading = false;
        
        // 3. UI 및 애니메이터 동기화
        if (magazineBinder != null)
            magazineBinder.Unbind();
        
        RefreshAllStates();

        if (animator != null)
        {
            animator.ResetTrigger(TRIGGER_FIRE);
            animator.ResetTrigger(TRIGGER_RELOAD);
            animator.ResetTrigger(TRIGGER_EQUIP);
            animator.Rebind();
        }
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

        if (magazineBinder == null)
            magazineBinder = FindObjectOfType<WeaponMagazineUIBinder>(true);

        ApplyAnimatorBools();
        ApplyAimDirectionParams();
        PushWeaponContext();
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
            if (input.QuickSlot1)
                EquipSlot(0);
            if (input.QuickSlot2)
                EquipSlot(1);

            if (input.OffWeapon)
                UnEquipCurrent();
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

    private void EquipSlot(int slotIndex)
    {
        string targetWeaponId = inventory.GetWeaponId(slotIndex);
        if (string.IsNullOrEmpty(targetWeaponId))
            return;
        
        if (IsWeaponEquipped && currentSlotIndex == slotIndex)
        {
            UnEquipCurrent();
            return;
        }

        if (IsWeaponEquipped)
        {
            SaveCurrentWeaponState();
            UnbindWeaponEvents(currentWeapon);
            magazineBinder.Unbind();
        }

        currentSlotIndex = slotIndex;
        
        // 데이터베이스 및 레지스트리에서 정보 획득
        WeaponData wData = Databases.Instance.Weapon.GetData(targetWeaponId);
        WeaponBase wPrefab = prefabRegistry.GetPrefab(targetWeaponId);

        if (wData == null || wPrefab == null)
        {
            Debug.LogError($"[CombatCtrl] Failed to load weapon: {targetWeaponId}");
            return;
        }
        Debug.Log($"<color=cyan>[CombatCtrl]</color> ::: Found WeaponData: <b>{wData.DisplayName}</b> (Caliber: {wData.Caliber})");
        
        BulletData bData = null;
        if (Databases.Instance.Bullet.TryGetDefault(wData.Caliber, out var defaultBullet))
            bData = defaultBullet;

        // 무기 컨텍스트 준비
        UpdateWeaponContext();
        
        // 무기 생성 및 초기화
        currentWeapon = weaponRoot.Equip(wPrefab, wData, weaponContext);

        if (currentWeapon != null)
        {
            IsWeaponEquipped = true;
            currentWeapon.SetBullet(bData);
            
            if (savedAmmoByWeaponKey.TryGetValue(targetWeaponId, out int savedAmmo))
                currentWeapon.SetCurrentAmmo(savedAmmo);
            
            Debug.Log($"<color=white><b>[CombatCtrl] ::: SUCCESS!</b></color> Weapon '{wData.DisplayName}' equipped in Slot {slotIndex}.");
            
            animator.SetTrigger(TRIGGER_EQUIP);
            BindWeaponEvents(currentWeapon);
            magazineBinder.Bind(currentWeapon);

            RefreshAllStates();
        }
    }

    private void UnEquipCurrent()
    {
        if (currentWeapon == null)
            return;

        SaveCurrentWeaponState();
        animator.SetTrigger(TRIGGER_UNEQUIP);
        
        UnbindWeaponEvents(currentWeapon);
        magazineBinder.Unbind();
        weaponRoot.Unequip();

        currentWeapon = null;
        IsWeaponEquipped = false;
        
        RefreshAllStates();
    }

    private void SaveCurrentWeaponState()
    {
        if (currentWeapon != null && currentWeapon.Data != null)
        {
            savedAmmoByWeaponKey[currentWeapon.Data.Id] = currentWeapon.CurrentAmmo;
        }
    }

    private void UpdateWeaponContext()
    {
        if (weaponContext == null)
            weaponContext = new WeaponContext(aimProvider, transform, IsAiming);
        else
        {
            weaponContext.SetAimProvider(aimProvider);
            weaponContext.SetIsAiming(IsAiming);
        }
    }

    private void RefreshAllStates()
    {
        ApplyAnimatorBools();
        PushWeaponContext();
        RefreshCrosshairVisibility();
        RefreshRigWeights();
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

        aimLineController.SetAiming(aiming);
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
        
        BindAimProviderSafe();
        if (aimProvider == null)
            return;

        weaponContext.SetAimProvider(aimProvider);
        weaponContext.SetIsAiming(IsAiming);
        
        if (weaponRoot != null)
            weaponRoot.PushContext(weaponContext);

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

    private bool GetEquipToggleDown() => input.OffWeapon;
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

        bool isVisible = IsWeaponEquipped;
        bool shouldFollowMouse = false;
        
        if (cameraController != null)
        {
            // 1인칭: 무기 장착 시 CrosshairUI 표시 (중앙 고정)
            // 쿼터뷰: 무기 장착 시 CrosshairUI 표시 (마우스 추적)
            if (IsWeaponEquipped)
            {
                if (cameraController.Mode == CameraController.CameraMode.FirstPerson)
                {
                    shouldFollowMouse = false;
                }
                else if (cameraController.Mode == CameraController.CameraMode.QuarterView)
                {
                    shouldFollowMouse = true;
                }
            }
            cameraController.SetQuarterViewCursorHidden(isVisible  && cameraController.Mode == CameraController.CameraMode.QuarterView); 
        }
        crosshairUI.gameObject.SetActive(isVisible);
        crosshairUI.SetFollowMouse(shouldFollowMouse);

        if (isVisible && currentWeapon != null)
        {
            crosshairUI.SetContext(currentWeapon.Data, IsAiming);
        }
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

    public float GetCurrentWeaponAimMaxDistance(float fallback)
    {
        if (currentWeapon == null || currentWeapon.Data == null)
            return Mathf.Max(0.1f, fallback);
        
        float effective = Mathf.Max(currentWeapon.Data.EffectiveRange, 10f);
        return effective * 5f;
    }
    
    private void BindAimProviderSafe()
    {
        if (aimProviderSource != null && aimProviderSource is IAimProvider p)
            aimProvider = p;
        
        if (aimProvider == null)
            aimProvider = GetComponentInChildren<IAimProvider>(true);

        if (aimProvider == null)
            Debug.LogError("[PlayerCombatCtrl] BindAimProviderSafe failed: IAimProvider not found.");
    }
}
