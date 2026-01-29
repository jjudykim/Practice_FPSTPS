using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponDatabase
{
    private readonly string tableName;
    private readonly bool preferPesristent;
    private readonly bool logOnLoad;
    
     private readonly Dictionary<string, WeaponData> byId = new(StringComparer.OrdinalIgnoreCase);

    private bool isLoaded = false;
    private Task loadTask;

    public WeaponDatabase(string tableName = "Weapon", bool logOnLoad = true)
    {
        this.tableName = tableName;
        this.logOnLoad = logOnLoad;

        isLoaded = false;
        loadTask = null;
    }

    /// <summary>
    /// 로드가 필요하면 로드, 이미 로딩 중이면 그 Task를 반환
    /// </summary>
    public Task EnsureLoadedAsync()
    {
        if (isLoaded)
            return Task.CompletedTask;

        if (loadTask != null)
            return loadTask;

        loadTask = LoadInternalAsync();
        return loadTask;
    }

    public bool TryGet(string weaponId, out WeaponData data)
    {
        if (string.IsNullOrEmpty(weaponId))
        {
            data = null;
            return false;
        }

        return byId.TryGetValue(weaponId, out data);
    }

    public WeaponData GetOrNull(string weaponId)
    {
        if (TryGet(weaponId, out WeaponData data))
            return data;
        
        return null;
    }

    private async Task LoadInternalAsync()
    {
        byId.Clear();
        isLoaded = false;

        List<WeaponTsvRow> rows = await TryReadRowsAsync(false);

        int loaded = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            WeaponTsvRow row = rows[i];
            if (row == null)
                continue;

            if (string.IsNullOrEmpty(row.Id))
            {
                Debug.LogWarning($"[WeaponDatabase] Row {i}: missing Id. skipped.");
                continue;
            }

            WeaponData data = ConvertRowToWeaponData(row, i);
            if (data == null)
                continue;

            data.ValidateAndClamp();

            if (byId.ContainsKey(data.Id))
            {
                Debug.LogWarning($"[WeaponDatabase] Duplicated Id='{data.Id}'. Overwriting previous.");
                byId[data.Id] = data;
            }
            else
            {
                byId.Add(data.Id, data);
            }

            loaded++;
        }

        isLoaded = true;

        if (logOnLoad)
            Debug.Log($"[WeaponDatabase] Loaded {loaded} weapons from table '{tableName}'.");
    }

    private async Task<List<WeaponTsvRow>> TryReadRowsAsync(bool isStreamingAssetPath)
    {
        try
        {
            List<WeaponTsvRow> rows = await TSVReader.ReadTableAsync<WeaponTsvRow>(tableName, isStreamingAssetPath);
            return rows;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[WeaponDatabase] ReadTable failed from PersistentData. table='{tableName}'.\n{e}");
            return null;
        }
    }

    private WeaponData ConvertRowToWeaponData(WeaponTsvRow row, int rowIndex)
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

    private WeaponCaliber ParseCaliberOrDefault(string raw, int rowIndex, string weaponId)
    {
        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning($"[WeaponDatabase] Row {rowIndex} (Id='{weaponId}'): Caliber empty. Using None.");
            return WeaponCaliber.None;
        }

        // 예: "Rifle_556"
        if (Enum.TryParse(raw.Trim(), ignoreCase: true, out WeaponCaliber parsed))
            return parsed;

        Debug.LogWarning($"[WeaponDatabase] Row {rowIndex} (Id='{weaponId}'): Caliber '{raw}' invalid. Using None.");
        return WeaponCaliber.None;
    }
        
    
}