using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class WeaponBase : MonoBehaviour, IWeapon
{
    [Header("RuntimeData (Injected)")] [SerializeField]
    private WeaponData data;

    [SerializeField] private WeaponContext context;

    [Header("Ammo")] [SerializeField] private BulletData bullet;

    [Header("Model")] [SerializeField] private GameObject model;

    [Header("IK")] public Transform AttachPoint;
    [Range(0.0f, 1.0f)] public float ikWeight;

    [Header("Refs")] [SerializeField] private Transform muzzle;
    [SerializeField] private HitscanShooter hitscanShooter;

    // Runtime
    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;

    private Coroutine reloadCoroutine;

    // Fire 성공 시 호출 이벤트
    public event Action OnShotFired;

    public GameObject Model => model;
    public WeaponData Data => data;

    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;

    private void Awake()
    {
        if (hitscanShooter == null)
            hitscanShooter = GetComponentInChildren<HitscanShooter>();

        if (data != null)
        {
            data.ValidateAndClamp();
            currentAmmo = data.MagazineSize;
        }

        if (bullet != null && string.IsNullOrEmpty(bullet.Id))
            bullet = null;

        if (bullet != null)
            bullet.ValidateAndClamp();

        EnsureBulletCompatibilityOrNull();
        ResolveHardpoints();
    }

    // 장착 시 1회 주입용 초기화 함수
    public void Init(WeaponData weaponData, WeaponContext weaponContext)
    {
        data = weaponData;
        context = weaponContext;

        if (data != null)
        {
            data.ValidateAndClamp();
            currentAmmo = data.MagazineSize;
        }

        isReloading = false;
        nextFireTime = 0f;

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        EnsureBulletCompatibilityOrNull();

        ResolveHardpoints();
        BindAimProviderHardpoints();
    }

    public void SetBullet(BulletData bulletData)
    {
        if (bulletData == null || string.IsNullOrEmpty(bulletData.Id))
        {
            bullet = null;
            return;
        }

        bullet = bulletData;
        bullet.ValidateAndClamp();
        EnsureBulletCompatibilityOrNull();
    }

    public void SetContext(WeaponContext weaponContext)
    {
        context = weaponContext;
        ResolveHardpoints();
        BindAimProviderHardpoints();
    }

    public void TriggerDown()
    {
        TryFire();
    }

    public void TriggerHold()
    {
        if (data.isAutomatic == false)
            return;

        TryFire();
    }

    public void TriggerUp()
    {
        // 차지샷, 볼트액션 등 ... (추가 구현 포인트)
    }

    public void Reload()
    {
        if (data == null)
            return;

        if (isReloading)
            return;

        if (currentAmmo >= data.MagazineSize)
            return;

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        reloadCoroutine = StartCoroutine(CoReload());
    }

    public void CancelReload()
    {
        if (isReloading == false)
            return;

        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;

        // 취소 사운드/애니메이션 처리 훅
        Debug.Log("[WeaponBase] ::: Reload Canceled");
    }

    private IEnumerator CoReload()
    {
        isReloading = true;

        yield return new WaitForSeconds(data.ReloadTime);

        currentAmmo = data.MagazineSize;

        isReloading = false;
        reloadCoroutine = null;
    }

    private void TryFire()
    {
        if (data == null)
            return;

        // Bullet / Caliber 체크 =======================
        if (bullet == null)
        {
            Debug.LogWarning("[WeaponBase] ::: Bullet is Null");
            return;
        }

        if (data.Caliber.IsCompatibleWith(bullet.Caliber) == false)
        {
            Debug.LogWarning("[WeaponBase] ::: Incompatible caliber");
            return;
        }


        // Aim / Shoot 관련 체크 =======================
        if (context.AimProvider == null)
        {
            Debug.LogWarning("[WeaponBase] ::: AimProvider is Null");
            return;
        }

        if (hitscanShooter == null)
        {
            Debug.LogError("[WeaponBase] ::: hitscanShooter is Null");
            return;
        }

        // Reload 중 발사 시 Reload 취소
        if (isReloading)
            CancelReload();

        if (Time.time < nextFireTime)
        {
            Debug.Log($"[WeaponBase] blocked: fireRate gate. now={Time.time:F3} next={nextFireTime:F3}");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("[WeaponBase] ammo empty -> Reload()");
            return;
        }


        // 발사 실행 지점
        bool isADS = context.IsADS;

        OnShotFired?.Invoke();

        float finalDamage = data.BaseDamage * Mathf.Max(0f, bullet.DamageMultiplier);
        GameObject attacker = transform.root.gameObject;
        hitscanShooter.Fire(attacker, context.AimProvider, muzzle, data, isADS, finalDamage);

        currentAmmo--;

        // 소음 이벤트
        EmitNoise(data.NoiseRadius);

        // 반동
        ApplyRecoil(data.VerticalRecoil, data.HorizontalRecoil, isADS);

        float interval = 1f / data.FireRate;
        nextFireTime = Time.time + interval;
    }

    private void EmitNoise(float radius)
    {
        if (radius <= 0f)
            return;

        // 나중에 NoiseSystem으로 이벤트 발행
    }

    private void ApplyRecoil(float vertical, float horizontal, bool isADS)
    {
        // 반동 값 발생까지만 책임지고, 반동 적용은 조준 컨트롤러에게 위임
        // TODO : IRecoilReceiver를 제작해서 호출
    }

    // ===============================
    //      Bullet Compatibility
    // ===============================
    private void EnsureBulletCompatibilityOrNull()
    {
        if (data == null || bullet == null)
            return;

        if (data.Caliber.IsCompatibleWith(bullet.Caliber) == false)
        {
            Debug.LogWarning($"[WeaponBase] ::: Incompatible caliber");
            bullet = null;
        }
    }

    private void ResolveHardpoints()
    {
        if (muzzle == null)
        {
            Transform found = transform.GetComponentInChildren<Transform>().Find("Muzzle");
            if (found != null)
                muzzle = found;
        }

        if (AttachPoint == null)
        {
            Transform found = transform.GetComponentInChildren<Transform>().Find("AttachPoint");
            if (found != null)
                AttachPoint = found;
        }
    }

    private void BindAimProviderHardpoints()
    {
        if (context == null || context.AimProvider == null)
        {
            Debug.LogWarning("[WeaponBase] BindAimProviderHardpoints: context or AimProvider is null.");
            return;
        }

        if (muzzle == null)
            ResolveHardpoints();

        if (muzzle == null)
        {
            Debug.LogWarning(
                "[WeaponBase] BindAimProviderHardpoints: muzzle is NULL. (Check child name/path: 'Muzzle')");
            return;
        }
        
        if (context.AimProvider is AimProviderRouter router)
        {
            router.SetMuzzle(muzzle);
            return;
        }
        
        if (context.AimProvider is QuarterViewAimProvider q)
        {
            q.SetMuzzle(muzzle);
            return;
        }
    }
}

