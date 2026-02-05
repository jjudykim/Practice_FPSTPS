using UnityEngine;

public class WeaponRoot : MonoBehaviour
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    public WeaponBase CurrentWeapon { get; private set; }
    

    public WeaponBase Equip(WeaponBase weaponPrefab, WeaponData weaponData, WeaponContext context)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("[WeaponRoot] Equip failed: weaponPrefab is null.");
            return null;
        }

        if (weaponData == null)
        {
            Debug.LogError("[WeaponRoot] Equip failed: weaponData is null.");
            return null;
        }

        if (context == null || context.AimProvider == null)
        {
            Debug.LogError("[WeaponRoot] Equip failed: context or AimProvider is null.");
            return null;
        }
        
        if (CurrentWeapon != null)
        {
            Destroy(CurrentWeapon.gameObject);
            CurrentWeapon = null;
        }
        
        CurrentWeapon = Instantiate(weaponPrefab, transform);
        
        CurrentWeapon.Init(weaponData, context);

        return CurrentWeapon;
    }
    
    public void PushContext(WeaponContext context)
    {
        if (CurrentWeapon == null)
            return;

        if (context == null)
            return;
        
        CurrentWeapon.SetContext(context);
    }

    public void Unequip()
    {
        if (CurrentWeapon == null)
            return;

        Destroy(CurrentWeapon.gameObject);
        CurrentWeapon = null;
    }

    public void ClearAllWeapons()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
