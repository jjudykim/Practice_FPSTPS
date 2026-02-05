using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponDatabase : TSVDatabase<WeaponData, WeaponTsvRow>
{
    public WeaponDatabase(string tableName = "Weapon", bool logOnLoad = true) 
        : base(tableName, logOnLoad, StringComparer.OrdinalIgnoreCase)
    {
    }
    
    public WeaponData GetData(string id)
    {
        return GetOrNull(id);
    }
    
    public bool TryGetData(string id, out WeaponData data)
    {
        return TryGet(id, out data);
    }
    
    protected override WeaponData ConvertRowToData(WeaponTsvRow row, int rowIndex)
    {
        WeaponData d = new WeaponData();

        d.Id = row.Id;
        d.DisplayName = row.DisplayName;

        d.Weight = row.Weight;

        // Caliber: string -> enum
        d.Caliber = ParseCaliberOrDefault(row.Caliber, rowIndex, row.Id);

        d.BaseDamage = row.BaseDamage;
        d.FireRate = row.FireRate;
        d.MagazineSize = row.MagazineSize;
        d.ReloadTime = row.ReloadTime;

        d.EffectiveRange = row.EffectiveRange;
        d.CriticalDamageMultiplier = row.CriticalDamageMultiplier;
        d.NoiseRadius = row.NoiseRadius;

        d.MoveSpeedMultiplier = row.MoveSpeedMultiplier;
        d.ADS_MoveSpeedMultiplier = row.ADS_MoveSpeedMultiplier;
        d.ADS_Spread = row.ADS_Spread;

        d.VerticalRecoil = row.VerticalRecoil;
        d.HorizontalRecoil = row.HorizontalRecoil;
        
        d.isAutomatic = row.isAutomatic;

        return d;
    }

    protected override void ValidateData(WeaponData data, WeaponTsvRow row, int rowIndex)
    {
        data.ValidateAndClamp();
    }
    
    private Caliber ParseCaliberOrDefault(string raw, int rowIndex, string weaponId)
    {
        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning($"[WeaponDatabase] Row {rowIndex} (Id='{weaponId}'): Caliber empty. Using None.");
            return Caliber.None;
        }
        
        if (Enum.TryParse(raw.Trim(), ignoreCase: true, out Caliber parsed))
            return parsed;

        Debug.LogWarning($"[WeaponDatabase] Row {rowIndex} (Id='{weaponId}'): Caliber '{raw}' invalid. Using None.");
        return Caliber.None;
    }
}