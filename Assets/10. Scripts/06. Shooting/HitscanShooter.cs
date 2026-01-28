using UnityEngine;

public class HitscanShooter : MonoBehaviour
{
    [Header("Raycast Settings")] 
    [SerializeField] private LayerMask aimMask; // 조준점 계산용 (지형 / 적 등)
    [SerializeField] private LayerMask hitMask; // 실제 피격 판정 레이어

    [Header("Debug")] [SerializeField] private bool debugDraw = true;

    public void Fire(IAimProvider aimProvider, Transform muzzle, WeaponData data, bool isADS)
    {
        if (aimProvider == null || data == null)
            return;

        // 0) 발사 원점 (총구)
        Vector3 muzzleOrigin = muzzle.position;
        
        // 1) 카메라 중앙 Ray 확보 (조준 기준)
        Ray camRay = aimProvider.GetAimRay();

        // 사거리
        float aimMaxDistance = Mathf.Max(data.EffectiveRange, 10f) * 5f;

        // 2) "퍼짐"은 카메라 방향 기준으로 적용
        float spreadAngleDeg = isADS ? data.ADS_Spread : Mathf.Max(data.ADS_Spread * 3f, data.ADS_Spread);

        Vector3 camDir = camRay.direction.normalized;
        Vector3 camDirWithSpread = ApplySpread(camDir, spreadAngleDeg);

        Ray camSpreadRay = new Ray(camRay.origin, camDirWithSpread);
        
        Vector3 aimPoint;
        bool aimed = false;

        if (Physics.Raycast(camSpreadRay, out RaycastHit camHit, aimMaxDistance
                          , aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = camHit.point;
            aimed = true;
        }
        else
        {
            aimPoint = camSpreadRay.origin + camSpreadRay.direction * aimMaxDistance;
        }
        
        // 3) 총구 -> aimPoint로 발사 방향 보정
        Vector3 muzzleDir = (aimPoint - muzzleOrigin).normalized;

        // 4) 총구 앞 장애물 체크
        float toAimDistance = Vector3.Distance(muzzleOrigin, aimPoint);
        if (Physics.Raycast(muzzleOrigin, muzzleDir
                          , out RaycastHit muzzleBlockHit, toAimDistance
                          , hitMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = muzzleBlockHit.point;
        }
        
        // 5) 실제 피격 판정 (총구 기준 Ray로)
        Ray fireRay = new Ray(muzzleOrigin, (aimPoint - muzzleOrigin).normalized);
        float fireMaxDistance = aimMaxDistance;

        if (Physics.Raycast(fireRay, out RaycastHit hit, fireMaxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            float damage = ComputeDamageWithRange(data, hit.distance);

            // TODO : 치명타 / 헤드샷 판정 나중에 추가
            bool isCritical = false;
            if (isCritical)
                damage *= data.CriticalDamageMultiplier;

            Debug.Log($"[HitscanShooter] Hit={hit.collider.name}, dist={hit.distance:F1}, dmg={damage:F1}, aimed={aimed}, ads={isADS}");

            if (debugDraw)
                Debug.DrawLine(muzzleOrigin, hit.point, Color.red, 0.1f);
        }
        else
        {
            if (debugDraw)
                Debug.DrawLine(muzzleOrigin, muzzleOrigin + fireRay.direction * fireMaxDistance, Color.yellow, 0.1f);
        }
        
        if (debugDraw)
        {
            Debug.DrawRay(camRay.origin, camRay.direction * 3f, Color.green, 0.1f);
            Debug.DrawRay(camSpreadRay.origin, camSpreadRay.direction * 3f, Color.cyan, 0.1f);
            Debug.DrawRay(muzzleOrigin, (aimPoint - muzzleOrigin).normalized * 3f, Color.red, 0.1f);
        }
    }

    private static Vector3 ApplySpread(Vector3 dir, float spreadAngleDeg)
    {
        if (spreadAngleDeg <= 0f)
            return dir;

        float yaw = Random.Range(-spreadAngleDeg, spreadAngleDeg);
        float pitch = Random.Range(-spreadAngleDeg, spreadAngleDeg);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        return rot * dir;
    }

    private static float ComputeDamageWithRange(WeaponData data, float distance)
    {
        if (distance <= data.EffectiveRange)
            return data.BaseDamage;
        
        float over = distance - data.EffectiveRange;
        float falloffRange = Mathf.Max(data.EffectiveRange, 1f);    // 유효거리만큼 더 가면 절반 정도로 떨어지게
        float t = Mathf.Clamp01(over / falloffRange);
        
        float multiplier = Mathf.Lerp(1f, 0.5f, t);
        return data.BaseDamage * multiplier;
    }
}