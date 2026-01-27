using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombatController : MonoBehaviour
{
    private static readonly int IS_WEAPON_EQUIPPED = Animator.StringToHash("IsWeaponEquipped");
    private static readonly int IS_AIMING = Animator.StringToHash("IsAiming");
    
    private static readonly int TRIGGER_FIRE =  Animator.StringToHash("Fire");
    private static readonly int TRIGGER_RELOAD =  Animator.StringToHash("Reload");
    private static readonly int TRIGGER_EQUIP =  Animator.StringToHash("Equip");
    private static readonly int TRIGGER_UNEQUIP =  Animator.StringToHash("UnEquip");
    
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
    
    public bool IsWeaponEquipped { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsReloading { get; private set; }
    
    public bool IsBusyByRoll => (moveController != null) && moveController.IsRolling;
    private bool IsBusyByRunForFire => (moveController != null) && moveController.IsRunning;

    public bool CanFire => IsWeaponEquipped
                           && (IsBusyByRoll == false) 
                           && (IsBusyByRunForFire == false);

    public bool CanAim => IsWeaponEquipped
                          && (IsBusyByRoll == false);
    
    private InputManager input;
    private int upperBodyLayerIndex = -1;

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
    
    private void Update()
    {
        bool rolling = IsBusyByRoll;
        
        if (GetEquipToggleDown())
            ToggleEquip();

        if (IsWeaponEquipped == false)
        {
            if (IsAiming)
                SetAiming(false);

            return;
        }

        if (rolling)
        {
            if (IsAiming)
                SetAiming(false);
            
            if (currentWeapon != null)
                currentWeapon.CancelReload();
            
            ApplyAimDirectionParams();
            return;
        }

        TickAim();

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
            animator.SetTrigger(TRIGGER_EQUIP);
        }
        else
        {
            weaponRoot.Unequip();
            currentWeapon = null;
            animator.SetTrigger(TRIGGER_UNEQUIP);

            if (IsAiming)
                SetAiming(false);
        }
        
        ApplyAnimatorBools();
        PushWeaponContext();
        RefreshCrosshairVisibility();

        RefreshRigWeights();
        
        Debug.Log($"[PlayerCombatCtrl] ::: Equip = {IsWeaponEquipped}");
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

    private void SetAiming(bool aiming)
    {
        IsAiming = aiming;
        
        ApplyAnimatorBools();
        PushWeaponContext();
        RefreshCrosshairVisibility();
        
        RefreshRigWeights();

        cameraController.SetAiming(IsAiming);
        
        ApplyAimDirectionParams();
        
        Debug.Log($"[PlayerCombatCtrl] ::: Aiming = {IsAiming}");
    }

    private void PushWeaponContext()
    {
        if (currentWeapon == null)
            return;
        
        currentWeapon.UpdateContext(new WeaponContext(aimProvider
                                                    , transform
                                                    , IsAiming));
        
        crosshairUI.SetContext(currentWeapon.Data, IsAiming);
    }

    private void ApplyAimDirectionParams()
    {
        if (animator == null)
            return;

        if (lookController == null)
            return;

        Vector2 look = lookController.LookLocalDir;
        
        animator.SetFloat(AIM_X, look.x);
        animator.SetFloat(AIM_Y, look.y);
    }

    private void TickFire()
    {
        if (CanFire == false)
            return;

        if (GetFireHeld() == false)
            return;

        bool fireDown = GetFireDown();
        bool fireHeld = GetFireHeld();
        bool fireUp = GetFireUp();

        if (fireDown)
        {
            animator.SetTrigger(TRIGGER_FIRE);
            currentWeapon.TriggerDown();
            
            Debug.Log("[PlayerCombatCtrl] ::: Fire Down");
            crosshairUI.OnFired();
        }

        if (fireHeld)
        {
            animator.SetTrigger(TRIGGER_FIRE);
            currentWeapon.TriggerHold();
        }

        if (fireUp)
            currentWeapon.TriggerUp();
    }

    private void TryReload()
    {
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

    private bool GetFireDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool GetFireUp()
    {
        return Input.GetMouseButtonUp(0);
    }

    private bool GetFireHeld()
    {
        if (input.Shoot)
            return true;

        return input.Shoot;
    }

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

    private void RefreshCrosshairVisibility()
    {
        if (crosshairUI == null)
            return;

        bool visible = false;

        if (cameraController != null)
        {
            visible = (cameraController.Mode == CameraController.CameraMode.FirstPerson) 
                      && IsWeaponEquipped;
        }
        
        crosshairUI.gameObject.SetActive(visible);
        
        if (visible == false && IsAiming)
            SetAiming(false);
    }

    private void RefreshRigWeights()
    {
        if (IsWeaponEquipped == false)
        {
            ApplyUpperBodyLayerWeight(0f);
            return;
        }

        if (IsAiming)
        {
            ApplyUpperBodyLayerWeight(upperBodyWeightAiming);
        }
        else
        {
            ApplyUpperBodyLayerWeight(upperBodyWeightEquipped);
        }
    }
}
