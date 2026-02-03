using UnityEngine;

public class CombatManager
{
    // Collider에서 IDamageable(부모 포함)을 찾아 데미지 적용
    public bool TryDealDamage(GameObject attacker, Collider hitCollider, float damage
                            , Vector3 hitPoint, Vector3 hitNormal, HitPart hitPart
                            , string source)
    {
        if (hitCollider == null)
            return false;

        IDamageable victim = hitCollider.GetComponentInParent<IDamageable>();
        if (victim == null)
            return false;

        MonoBehaviour victimMb = victim as MonoBehaviour;
        if (victimMb == null)
            return false;

        return TryDealDamage(attacker, victimMb.gameObject, damage, hitPoint, hitNormal, hitPart, source);
    }

    // 피해자 GameObject에서 IDamageable(부모 포함)을 찾아 데미지 적용
    public bool TryDealDamage(GameObject attacker, GameObject victimGO, float damage
                            , Vector3 hitPoint, Vector3 hitNormal, HitPart hitPart
                            , string source)
    {
        if (victimGO == null)
            return false;

        IDamageable victim = victimGO.GetComponentInParent<IDamageable>();
        if (victim == null)
            return false;

        if (victim.IsAlive == false)
            return false;

        MonoBehaviour victimMb = victim as MonoBehaviour;
        if (victimMb == null)
            return false;

        DamageInfo info = new DamageInfo(
            attacker,
            victimMb.gameObject,
            damage,
            hitPoint,
            hitNormal,
            hitPart,
            source
        );

        victim.ApplyDamage(info);
        return true;
    }
}
