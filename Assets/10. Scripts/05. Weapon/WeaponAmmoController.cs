using UnityEngine;

// TODO: 나중에 탄약 인벤토리 확장할 때 활용할 클래스
public class WeaponAmmoController
{
    private WeaponData weapon;
    private BulletData loadedBullet;

    public BulletData LoadedBullet => loadedBullet;

    public void Init(WeaponData weaponData)
    {
        weapon = weaponData;
        loadedBullet = null;
    }

    public bool TryLoadBullet(BulletData bullet)
    {
        if (weapon == null)
        {
            Debug.LogError("[WeaponAmmoController] ::: weapon in null");
            return false;
        }

        if (bullet == null)
        {
            Debug.LogWarning("[WeaponAmmoController] bullet is null.");
            return false;
        }

        if (weapon.Caliber.IsCompatibleWith(bullet.Caliber) == false)
        {
            Debug.LogWarning($"[WeaponAmmoController] Incompatible bullet.");
            return false;
        }

        loadedBullet = bullet;
        return true;
    }

    public bool HasValidLoadedBullet()
    {
        if (weapon == null || loadedBullet == null)
            return false;

        return weapon.Caliber.IsCompatibleWith(loadedBullet.Caliber);
    }
}