using System;
using UnityEngine.Serialization;

[Serializable]
public class WeaponData
{
    public string Id;
    public string DisplayName;
    
    public float Weight;
    public Caliber Caliber;
    
    public float BaseDamage;
    public float FireRate; // RPS
    
    public int MagazineSize;
    public float ReloadTime;
    
    public float EffectiveRange;
    
    public float CriticalDamageMultiplier;
    
    public float NoiseRadius;
    
    public float MoveSpeedMultiplier;
    public float ADS_MoveSpeedMultiplier;
    
    public float ADS_Spread;
    
    public float VerticalRecoil;
    public float HorizontalRecoil;

    public bool isAutomatic;

    public void ValidateAndClamp()
    {
        if (FireRate < 0.01f)
            FireRate = 0.01f;

        if (MagazineSize < 0)
            MagazineSize = 0;
        if (ReloadTime < 0)
            ReloadTime = 0f;
        
        if (BaseDamage < 0f)
            BaseDamage = 0f;
        if (CriticalDamageMultiplier < 1f)
            CriticalDamageMultiplier = 1f;
        
        if (EffectiveRange < 0f)
            EffectiveRange = 0f;
        
        if (NoiseRadius < 0f)
            NoiseRadius = 0f;
        
        if (MoveSpeedMultiplier < 0.1f)
            MoveSpeedMultiplier = 0.1f;
        if (ADS_MoveSpeedMultiplier  < 0.1f)
            ADS_MoveSpeedMultiplier  = 0.1f;
        
        if (ADS_Spread < 0f)
            ADS_Spread = 0f;
        
        if (VerticalRecoil < 0f)
            VerticalRecoil = 0f;
        if (HorizontalRecoil < 0f)
            HorizontalRecoil = 0f;
    }

#if UNITY_EDITOR
    public static WeaponData CreateTestData()
    {
        WeaponData d = new WeaponData();

        d.BaseDamage = 10f;
        d.FireRate = 5f;
        d.MagazineSize = 30;
        d.ReloadTime = 1.5f;

        d.EffectiveRange = 50f;
        d.ADS_Spread = 0.5f;

        d.VerticalRecoil = 1.0f;
        d.HorizontalRecoil = 0.5f;

        d.NoiseRadius = 10f;
        d.CriticalDamageMultiplier = 1.5f;

        d.isAutomatic = true;

        return d;
    }
#endif
}