using System.Collections;
using UnityEngine;

public class WeaponBase : MonoBehaviour, IWeapon
    {
        [Header("RuntimeData (Injected)")] 
        [SerializeField] private WeaponData data;
        [SerializeField] private WeaponContext context;

        [Header("Model")]
        [SerializeField] private GameObject model;
        
        [Header("IK")]
        public Transform AttachPoint;
        [Range(0.0f, 1.0f)] public float ikWeight;
        
        [Header("Refs")] 
        [SerializeField] private Transform muzzle;
        [SerializeField] private HitscanShooter hitscanShooter;
        
        // Runtime
        private int currentAmmo;
        private bool isReloading;
        private float nextFireTime;
        
        private Coroutine reloadCoroutine;

        public GameObject Model => model;
        public WeaponData Data => data;
        
        public bool IsRealoading => isReloading;
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
        }

        public void SetContext(WeaponContext weaponContext)
        {
            context = weaponContext;
        }

        public void TriggerDown()
        {
            if (data.isAutomatic == false)
            {
                TryFire();
                return;
            }

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

            if (isReloading)
                return;

            if (Time.time < nextFireTime)
            {
                Debug.Log($"[WeaponBase] blocked: fireRate gate. now={Time.time:F3} next={nextFireTime:F3}");
                return;
            }

            if (currentAmmo <= 0)
            {
                Debug.Log("[WeaponBase] ammo empty -> Reload()");
                Reload();
                return;
            }
            
            
            // 발사 실행 지점
            bool isADS = context.IsADS;
            
            Debug.Log("[WeaponBase] calling HitscanShooter.Fire()");
            hitscanShooter.Fire(context.AimProvider, muzzle, data, isADS);
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

        private void ResolveHardpoints()
        {
            if (muzzle == null)
            {
                Transform found = transform.Find("Muzzle");
                if (found != null)
                    muzzle = found;
            }

            if (AttachPoint == null)
            {
                Transform found = transform.Find("AttachPoint");
                if (found != null)
                    AttachPoint = found;
            }
        }
    }

