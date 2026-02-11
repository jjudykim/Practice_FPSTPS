using System;
using System.Globalization;

[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public string lastSaveTimeUtcIso = "";
    public GameProgressData progress = new GameProgressData();
    

    public static SaveData CreateNew(int version = 1)
    {
        var data = new SaveData
        {
            saveVersion = version,
            lastSaveTimeUtcIso = "",
            progress = GameProgressData.CreateNew()
        };
        
        return data;
    }

    public void MarkSavedNowUtc()
    {
        lastSaveTimeUtcIso = DateTime.UtcNow.ToString("O");
    }

    public bool TryGetLastSaveUtc(out DateTime utc)
    {
        utc = default;

        if (string.IsNullOrEmpty(lastSaveTimeUtcIso))
            return false;

        return DateTime.TryParse(lastSaveTimeUtcIso
                               , CultureInfo.InvariantCulture
                               , DateTimeStyles.RoundtripKind
                               , out utc);
    }

    public string GetLastSaveLocalText(string format = "yyyy-MM-dd HH:mm:ss")
    {
        if (TryGetLastSaveUtc(out DateTime utc) == false)
            return "";

        return utc.ToLocalTime().ToString(format);
    }
}