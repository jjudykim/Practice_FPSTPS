using System;
using System.IO;
using UnityEngine;

public class UserDataManager
{
    private string saveFolderName = "Save";
    private string saveFileNameWithoutExt = "userData";

    public UserData Data { get; private set; }

    public event Action<UserData> OnLoaded;
    public event Action<UserData> OnSaved;

    public event Action<UserData> OnDataChanged;

    private bool isDirty;

    private void Awake()
    {
        LoadOrCreateDefault();
    }

    public string GetSavePathWithoutExt()
    {
        string dir = Path.Combine(Application.persistentDataPath, saveFolderName);
        string pathWithoutExt = Path.Combine(dir, saveFileNameWithoutExt);
        return pathWithoutExt;
    }

    public string GetSaveFullPath()
    {
        return GetSavePathWithoutExt() + ".json";
    }

    public void LoadOrCreateDefault()
    {
        string pathWithoutExt = GetSavePathWithoutExt();

        UserData loaded = JsonReader.Load<UserData>(pathWithoutExt);

        if (loaded == null)
        {
            Data = UserData.CreateDefault();
            isDirty = true;
            Debug.LogWarning($"[UserDataManager] ::: Load failed.");
        }
        else
        {
            Data = loaded;
            isDirty = false;
            Debug.LogWarning($"[UserDataManager] ::: Load success.");
        }
        
        OnLoaded?.Invoke(Data);
        OnDataChanged?.Invoke(Data);
    }

    // 강제 저장
    public bool Save()
    {
        if (Data == null)
        {
            Debug.LogError("[UserDataManager] Save failed.");
            return false;
        }
        
        Data.MarkSavedNowUtc();

        string pathWithoutExt = GetSavePathWithoutExt();
        JsonWriter.Save(Data, pathWithoutExt);

        isDirty = false;
        OnSaved?.Invoke(Data);

        Debug.Log($"[UserDataManager] Save success.");
        return true;
    }

    public bool SaveIfDirty()
    {
        if (!isDirty)
            return false;

        return Save();
    }

    public void NotifyDataChanged()
    {
        if (Data == null)
            return;

        isDirty = true;
        OnDataChanged?.Invoke(Data);
    }

    public void ResetToDefault(bool saveImmediately = false)
    {
        Data = UserData.CreateDefault();
        isDirty = true;
        
        OnDataChanged?.Invoke(Data);
        
        Debug.Log($"[UserDataManager] Reset to default.");

        if (saveImmediately)
            Save();
    }

    public void AddGold(int amount)
    {
        if (Data == null)
            return;

        Data.gold += amount;
        NotifyDataChanged();
    }

    public void SetChapter(int chapter, int stage)
    {
        if (Data == null)
            return;

        Data.chapterIndex = chapter;
        NotifyDataChanged();
    }
}