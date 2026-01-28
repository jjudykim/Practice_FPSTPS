using UnityEngine;

public class WeaponRoot : MonoBehaviour
{
    [Header("Optional Default Weapon")]
    [SerializeField] private WeaponBase defaultWeaponPrefab;
    [SerializeField] private bool equipDefaultOnStart = false;

    public WeaponBase CurrentWeapon { get; private set; }

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

        WeaponBase instance = Instantiate(weaponPrefab, transform);

        CurrentWeapon = instance;
        
        // 임시로 WeaponData & Context 생성
        WeaponData tempData = WeaponData.CreateTestData();
        WeaponContext tempContext = new WeaponContext(null, transform, false);
        
        CurrentWeapon.Init(tempData, tempContext);

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
