using System;

[Serializable]
public class WeaponTsvRow
{
    // ===========================
    // Key / Display
    // ===========================
    public string Id { get; set; }
    public string DisplayName { get; set; }

    // ===========================
    // Stats
    // ===========================
    public float Weight { get; set; }
    
    public string Caliber { get; set; }

    public float BaseDamage { get; set; }
    public float FireRate { get; set; }            // Rounds Per Second
    public int MagazineSize { get; set; }
    public float ReloadTime { get; set; }

    public float EffectiveRange { get; set; }
    public float CriticalDamageMultiplier { get; set; }
    public float NoiseRadius { get; set; }

    public float MoveSpeedMultiplier { get; set; }
    public float ADS_MoveSpeedMultiplier { get; set; }
    public float ADS_Spread { get; set; }

    public float VerticalRecoil { get; set; }
    public float HorizontalRecoil { get; set; }
    
    public bool isAutomatic { get; set; }
}