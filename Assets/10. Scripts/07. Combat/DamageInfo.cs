using UnityEngine;

public enum HitPart
{
    None = 0,
    Body = 1,
    Head = 2,
}

public struct DamageInfo
{
    public GameObject Attacker;         // 공격자
    public GameObject Victim;           // 피해자
    public float Damage;                // 데미지
    public Vector3 HitPoint;            // 맞은 지점
    public Vector3 HitNormal;           // 맞은 면의 노멀
    public HitPart HitPart;             // 맞은 부위 (Head / Body)
    public string Source;

    public DamageInfo(GameObject attacker, GameObject victim, float damage
                    , Vector3 hitPoint, Vector3 hitNormal, HitPart hitPart
                    , string source)
    {
        Attacker = attacker;
        Victim = victim;
        Damage = damage;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        HitPart = hitPart;
        Source = source;
    }
}