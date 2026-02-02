using UnityEngine;

public class HitscanShooter : MonoBehaviour
{
    [Header("Raycast Settings")] 
    [SerializeField] private LayerMask aimMask; // 조준점 계산용 (지형 / 적 등)
    [SerializeField] private LayerMask hitMask; // 실제 피격 판정 레이어

    [Header("Debug")] 
    [SerializeField] private bool debugDraw = true;

    private const QueryTriggerInteraction HIT_QUERY = QueryTriggerInteraction.Collide;
    
    
    // 데미지 계수 적용 Ver
    public void Fire(GameObject attacker, IAimProvider aimProvider, Transform muzzle, WeaponData weaponData, bool isADS, float finalDamage)
    {
        if (aimProvider == null || weaponData == null || muzzle == null)
        {
            Debug.LogWarning($"[HitscanShooter] Fire blocked. aimProvider={(aimProvider!=null)}, weaponData={(weaponData!=null)}, muzzle={(muzzle!=null)}");
            return;
        }
        
         // 0) 발사 원점 (총구)
        Vector3 muzzleOrigin = muzzle.position;

        // 1) 카메라 중앙 Ray 확보 (조준 기준)
        Ray camRay = aimProvider.GetAimRay();

        // 사거리
        float aimMaxDistance = Mathf.Max(weaponData.EffectiveRange, 10f) * 5f;

        // 2) "퍼짐"은 카메라 방향 기준으로 적용
        float spreadAngleDeg = isADS
            ? weaponData.ADS_Spread
            : Mathf.Max(weaponData.ADS_Spread * 3f, weaponData.ADS_Spread);

        Vector3 camDir = camRay.direction.normalized;
        Vector3 camDirWithSpread = ApplySpread(camDir, spreadAngleDeg);

        Ray camSpreadRay = new Ray(camRay.origin, camDirWithSpread);

        Vector3 aimPoint;
        bool aimed = false;

        if (Physics.Raycast(camSpreadRay, out RaycastHit camHit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
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
        if (Physics.Raycast(muzzleOrigin, muzzleDir, out RaycastHit muzzleBlockHit, toAimDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = muzzleBlockHit.point;
        }

        // 5) 실제 피격 판정 (총구 기준 Ray로)
        Ray fireRay = new Ray(muzzleOrigin, (aimPoint - muzzleOrigin).normalized);
        float fireMaxDistance = aimMaxDistance;

        if (Physics.Raycast(fireRay, out RaycastHit hit, fireMaxDistance, hitMask, HIT_QUERY))
        {

            float damage = ComputeDamageWithRange(finalDamage, weaponData.EffectiveRange, hit.distance);

            HitPart hitPart = HitPart.Body;
            float partMultiplier = 1.0f;

            Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                hitPart = hitbox.Part;
                partMultiplier = hitbox.DamageMultiplier;
            }

            float final = damage * partMultiplier;

            Debug.Log($"[HitscanShooter] Hit={hit.collider.name}, part={hitPart}, dist={hit.distance:F1}, dmg={final:F1} (base={damage:F1}*{partMultiplier:F1}), aimed={aimed}, ads={isADS}");

            if (Managers.Instance.Combat != null)
            {
                Managers.Instance.Combat.TryDealDamage(
                    attacker: attacker,
                    hitCollider: hit.collider,
                    damage: final,
                    hitPoint: hit.point,
                    hitNormal: hit.normal,
                    hitPart: hitPart,
                    source: hitPart == HitPart.Head ? "HitscanHeadshot" : "HitscanBody"
                );
            }
            else
            {
                Debug.LogWarning("[HitscanShooter] CombatSystem.Instance is null.");
            }

            if (debugDraw)
                Debug.DrawLine(muzzleOrigin, hit.point, Color.red, 0.1f);
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
    
    private static float ComputeDamageWithRange(float baseDamage, float effectiveRange, float distance)
    {
        if (distance <= effectiveRange)
            return baseDamage;

        float over = distance - effectiveRange;
        float falloffRange = Mathf.Max(effectiveRange, 1f);
        float t = Mathf.Clamp01(over / falloffRange);

        float multiplier = Mathf.Lerp(1f, 0.5f, t);
        return baseDamage * multiplier;
    }
}