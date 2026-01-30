using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletDatabase : TSVDatabase<BulletData, BulletTsvRow>
{
    private readonly Dictionary<Caliber, BulletData> defaultByCaliber = new Dictionary<Caliber, BulletData>();

    public BulletDatabase(string table = "Bullet", bool logOnLoad = true) 
        : base(table, logOnLoad, StringComparer.OrdinalIgnoreCase)
    {
    }
    
    public bool TryGetDefault(Caliber caliber, out BulletData bullet)
    {
        if (caliber == Caliber.None)
        {
            bullet = null;
            return false;
        }

        return defaultByCaliber.TryGetValue(caliber, out bullet) && bullet != null;
    }

    protected override BulletData ConvertRowToData(BulletTsvRow row, int rowIndex)
    {
        BulletData d = new BulletData();

        d.Id = row.Id;
        d.DisplayName = row.DisplayName;

        d.Weight = row.Weight;

        // Caliber: string -> enum (공용 Caliber)
        d.Caliber = ParseCaliberOrDefault(row.Caliber, rowIndex, row.Id);

        d.DamageMultiplier = row.DamageMultiplier;

        return d;
    }
    
    protected override void ValidateData(BulletData data, BulletTsvRow row, int rowIndex)
    {
        data.ValidateAndClamp();

        if (data.Caliber == Caliber.None)
            return;

        if (row.IsDefault)
        {
            defaultByCaliber[data.Caliber] = data;
            return;
        }

        if (defaultByCaliber.ContainsKey(data.Caliber) == false)
        {
            defaultByCaliber.Add(data.Caliber, data);
        }
    }
    
    private Caliber ParseCaliberOrDefault(string raw, int rowIndex, string bulletId)
    {
        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning($"[BulletDatabase] ::: Row {rowIndex} (Id='{bulletId}'): Caliber empty. Using None.");
            return Caliber.None;
        }

        if (Enum.TryParse(raw.Trim(), ignoreCase: true, out Caliber parsed))
            return parsed;

        Debug.LogWarning($"[BulletDatabase] ::: Row {rowIndex} (Id='{bulletId}'): Caliber '{raw}' invalid. Using None.");
        return Caliber.None;
    }

}