using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    [SerializeField] private HitPart part = HitPart.Body;
    [SerializeField] private float damageMultiplier = 1.0f;

    public HitPart Part => part;
    public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (part == HitPart.Head && damageMultiplier < 1.5f)
            damageMultiplier = 2.0f;

        if (part == HitPart.Body && damageMultiplier <= 0f)
            damageMultiplier = 1.0f;
    }
#endif
}
