using System;
using UnityEngine;

[Serializable]
public class BulletData
{
    public string Id;
    public string DisplayName;

    public float Weight;
    public Caliber Caliber;

    public float DamageMultiplier = 1f;

    public void ValidateAndClamp()
    {
        if (Weight < 0f)
            Weight = 0f;
        if (DamageMultiplier < 0f)
            DamageMultiplier = 0f;
        
        if (Caliber == Caliber.None)
            Debug.LogWarning($"[BulletData] Caliber is None. id={Id}");
    }
}