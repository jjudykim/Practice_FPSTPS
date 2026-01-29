using UnityEngine;

public class WeaponRoot : MonoBehaviour
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    public WeaponBase CurrentWeapon { get; private set; }
    
    [Header("Optional Default Weapon")]
    [SerializeField] private WeaponBase defaultWeaponPrefab;
    [SerializeField] private bool equipDefaultOnStart = false;

    private void Awake()
    {
        weaponDatabase = Databases.Instance.Weapon;
    }

    private void Start()
    {
        if (equipDefaultOnStart && defaultWeaponPrefab != null)
        {
            Equip(defaultWeaponPrefab);
        }
    }

    public WeaponBase Equip(WeaponBase weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("[WeaponRoot] Equip failed. weaponPrefab is null.");
            return null;
        }

        Unequip();

        // 무기 인스턴스 생성
        WeaponBase instance = Instantiate(weaponPrefab, transform);
        CurrentWeapon = instance;

        // 1) WeaponId 확인
        WeaponIdBinding binding = CurrentWeapon.GetComponent<WeaponIdBinding>();
        if (binding == null || string.IsNullOrEmpty(binding.WeaponId))
        {
            Debug.LogError("[WeaponRoot] ::: WeaponIdBinding missing or WeaponId empty.");
            Unequip();
            return null;
        }

        if (Databases.Instance.IsLoaded == false)
        {
            Debug.LogWarning("[WeaponRoot] Databases is not loaded yet.");
        }
        
        if (Databases.Instance.Weapon == null)
        {
            Debug.LogError("[WeaponRoot] ::: WeaponDatabase is null.");
            Unequip();
            return null;
        }
        
        if (Databases.Instance.Weapon.TryGet(binding.WeaponId, out WeaponData weaponData) == false 
            || weaponData == null)
        {
            Debug.LogError($"[WeaponRoot] WeaponData not found. WeaponId='{binding.WeaponId}'.");
            Unequip();
            return null;
        }

        WeaponContext tempContext = new WeaponContext(null, transform, false);
        CurrentWeapon.Init(weaponData, tempContext);
        
        return CurrentWeapon;
    }

    public void Unequip()
    {
        if (CurrentWeapon == null)
            return;

        Destroy(CurrentWeapon.gameObject);
        CurrentWeapon = null;
    }
}
