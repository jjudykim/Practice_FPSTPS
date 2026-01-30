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
            Debug.Log($"[WeaponRoot] DB Loaded? Weapon={Databases.Instance.Weapon.IsLoaded}, Bullet={Databases.Instance.Bullet.IsLoaded}");
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

        // 1) 무기 인스턴스 생성
        WeaponBase instance = Instantiate(weaponPrefab, transform);
        CurrentWeapon = instance;

        // 2) WeaponId 확인
        WeaponIdBinding binding = CurrentWeapon.GetComponent<WeaponIdBinding>();
        if (binding == null || string.IsNullOrEmpty(binding.WeaponId))
        {
            Debug.LogError("[WeaponRoot] ::: WeaponIdBinding missing or WeaponId empty.");
            Unequip();
            return null;
        }
        
        // 3) DB 참조 가져오기
        WeaponDatabase weaponDb = Databases.Instance != null ? Databases.Instance.Weapon : null;
        BulletDatabase bulletDb = Databases.Instance != null ? Databases.Instance.Bullet : null;
        
        if (weaponDb == null)
        {
            Debug.LogError("[WeaponRoot] WeaponDatabase is null.");
            Unequip();
            return null;
        }
        
        // 4) WeaponData 로드
        if (weaponDb.TryGet(binding.WeaponId, out WeaponData weaponData) == false 
            || weaponData == null)
        {
            Debug.LogError($"[WeaponRoot] WeaponData not found. WeaponId='{binding.WeaponId}'.");
            Unequip();
            return null;
        }

        WeaponContext context = new WeaponContext(null, transform, false);
        CurrentWeapon.Init(weaponData, context);

        if (string.IsNullOrEmpty(binding.BulletId) == false)
        {
            if (bulletDb == null)
            {
                Debug.LogError("[WeaponRoot] ::: BulletDatabase is null.");
            }
            else if (bulletDb.TryGet(binding.BulletId, out BulletData bulletData) == false
                     || bulletData == null)
            {
                Debug.LogError($"[WeaponRoot] ::: BulletData not found. BulletId='{binding.BulletId}'.");
            }
            else
            {
                CurrentWeapon.SetBullet(bulletData);
            }
        }
        else
        {
            if (bulletDb.TryGetDefault(weaponData.Caliber, out BulletData defaultBullet))
                instance.SetBullet(defaultBullet);
            else
                Debug.LogWarning($"[WeaponRoot] No default bullet for caliber");
        }

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
