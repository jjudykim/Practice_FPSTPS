using System;
using System.Collections;
using UnityEngine;

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
    [SerializeField] private jjudy.Weapon currentWeapon;
    [SerializeField] private PlayerLookController lookController;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private string upperBodyLayerName = "UpperBody";
    
    [SerializeField] private bool aimHold = true;
    [SerializeField] private float fireCooldown = 0.12f;
    [SerializeField] private float reloadDuration = 1.2f;

    public bool IsWeaponEquipped { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsReloading { get; private set; }
    
    public bool IsBusyByRoll => (moveController != null) && moveController.IsRolling;
    private bool IsBusyByRunForFire => (moveController != null) && moveController.IsRunning;

    public bool CanFire => IsWeaponEquipped
                           && (IsReloading == false) 
                           && (IsBusyByRoll == false) 
                           && (IsBusyByRunForFire == false);

    public bool CanAim => IsWeaponEquipped
                          && (IsReloading == false)
                          && (IsBusyByRoll == false);
    
    private InputManager input;
    private int upperBodyLayerIndex = -1;

    private float lastFireTime = -999f;
    private Coroutine reloadCo;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        input = Managers.Instance.Input;

        if (animator != null && string.IsNullOrEmpty(upperBodyLayerName) == false)
            upperBodyLayerIndex = animator.GetLayerIndex(upperBodyLayerName);

        ApplyWeaponVisual(IsWeaponEquipped);
        ApplyUpperBodyLayerWeight(IsWeaponEquipped);
        ApplyAnimatorBools();
        ApplyAimDirectionParams();
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
            
            if (IsReloading)
                CancelReload();
            
            ApplyAimDirectionParams();
            return;
        }

        TickAim();

        if (GetReloadDown())
        {
            TryReload();
        }

        TickFire();
        
        ApplyAimDirectionParams();
    }

    private void ToggleEquip()
    {
        IsWeaponEquipped = !IsWeaponEquipped;

        ApplyWeaponVisual(IsWeaponEquipped);
        ApplyUpperBodyLayerWeight(IsWeaponEquipped);

        ApplyAnimatorBools();

        if (IsWeaponEquipped)
            animator.SetTrigger(TRIGGER_EQUIP);
        else
        {
            animator.SetTrigger(TRIGGER_UNEQUIP);

            if (IsAiming)
                SetAiming(false);

            CancelReload();
        }

        Debug.Log($"[PlayerCombatCtrl] ::: Equip = {IsWeaponEquipped}");
    }

    private void ApplyWeaponVisual(bool equipped)
    {
        currentWeapon.Model.SetActive(equipped);
    }

    private void TickAim()
    {
        if (CanAim == false)
        {
            if (IsAiming)
                SetAiming(false);
            return;
        }

        if (aimHold)
        {
            bool wantAim = GetAimHeld();
            if (wantAim != IsAiming)
                SetAiming(wantAim);
        }
    }

    private void SetAiming(bool aiming)
    {
        IsAiming = aiming;
        ApplyAnimatorBools();
        ApplyUpperBodyLayerWeight(IsWeaponEquipped || IsAiming);

        ApplyAimDirectionParams();
        
        Debug.Log($"[PlayerCombatCtrl] ::: Aiming = {IsAiming}");
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

        if (fireCooldown > 0f && Time.time < lastFireTime + fireCooldown)
            return;

        lastFireTime = Time.time;
        
        animator.SetTrigger(TRIGGER_FIRE);
        
        Debug.Log("[PlayerCombatCtrl] ::: Fire!!!!!!");
    }

    private void TryReload()
    {
        if (IsBusyByRoll)
            return;
        
        if (IsWeaponEquipped == false)
            return;

        if (IsReloading)
            return;

        if (IsAiming)
            SetAiming(false);

        IsReloading = true;
        ApplyAnimatorBools();
        
        animator.SetTrigger(TRIGGER_RELOAD);

        reloadCo = StartCoroutine(CoReload());
        Debug.Log("[PlayerCombatCtrl] ::: Reload Start");
    }

    private IEnumerator CoReload()
    {
        float t = 0f;
        while (t < reloadDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        IsReloading = false;
        ApplyAnimatorBools();

        reloadCo = null;
        Debug.Log("[PlayerCombatCtrl] ::: Reload End");
    }

    private void CancelReload()
    {
        if (reloadCo != null)
        {
            StopCoroutine(reloadCo);
            reloadCo = null;
        }

        if (IsReloading)
        {
            IsReloading = false;
            ApplyAnimatorBools();
        }
    }

    private void ApplyAnimatorBools()
    {
        if (animator == null)
            return;
        
        animator.SetBool(IS_WEAPON_EQUIPPED, IsWeaponEquipped);
        animator.SetBool(IS_AIMING, IsAiming);
    }

    private void ApplyUpperBodyLayerWeight(bool enabled)
    {
        if (animator == null)
            return;

        if (upperBodyLayerIndex < 0)
            return;
        
        animator.SetLayerWeight(upperBodyLayerIndex, enabled ? 1f : 0f);
    }

    private bool GetEquipToggleDown() => input.OnWeapon;
    private bool GetReloadDown() => input.Reload;

    private bool GetFireHeld()
    {
        if (input.Shoot)
            return true;

        return Input.GetMouseButton(0);
    }

    private bool GetAimHeld()
    {
        if (input.Aiming)
            return true;
        
        return Input.GetMouseButton(1);
    }

    private void OnAnimatorIK(int layerIndex)
    {
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
}
