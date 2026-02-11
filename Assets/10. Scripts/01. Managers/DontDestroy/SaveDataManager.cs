using System;
using System.IO;
using jjudy;
using UnityEngine;

public class SaveDataManager
{
    private const string DefaultSaveFolderName = "Save";
    private const string DefaultBaseFileName = "saveData";

    [SerializeField] private string saveFolderName = DefaultSaveFolderName;
    [SerializeField] private string baseFileNameWithoutExt = DefaultBaseFileName;

    public int CurrentSlotIndex { get; private set; } = 0;
    public SaveData Data { get; private set; }
    
    public ObservableIntValue Gold { get; } = new ObservableIntValue();
    public ObservableIntValue Level { get; } = new ObservableIntValue();
    public ObservableIntValue Exp { get; } = new ObservableIntValue();
    public ObservableIntValue MaxExp { get; } = new ObservableIntValue();
    
    public event Action<int, SaveData> OnLoaded;
    public event Action<int, SaveData> OnSaved;
    public event Action<int, SaveData> OnCreatedNew;
    
    
    // ================================
    //         Slot / Check File
    // ================================
    public void SetCurrentSlot(int slotIndex)
    {
        CurrentSlotIndex = Mathf.Max(0, slotIndex);
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSaveFullPath(slotIndex));
    }

    
    // ================================
    //     Create / Save / Load
    // ================================
    public SaveData CreateNew(int slotIndex, int saveVersion = 1, bool saveImmediately = false)
    {
        SetCurrentSlot(slotIndex);

        Data = SaveData.CreateNew(saveVersion);

        SyncObservables();

        OnCreatedNew?.Invoke(CurrentSlotIndex, Data);

        if (saveImmediately)
            Save(CurrentSlotIndex);

        return Data;
    }

    public SaveData Load(int slotIndex)
    {
        SetCurrentSlot(slotIndex);

        string pathWithoutExt = GetSavePathWithoutExt(CurrentSlotIndex);

        SaveData loaded = JsonReader.Load<SaveData>(pathWithoutExt);
        if (loaded == null)
        {
            Data = null;
            return null;
        }

        Data = loaded;

        SyncObservables();
        
        OnLoaded?.Invoke(CurrentSlotIndex, Data);
        return Data;
    }
    
    private void SyncObservables()
    {
        if (Data == null) return;
    
        var progress = Data.progress;
        Gold.Value = progress.currency.gold;
        Level.Value = progress.growth.level;
        Exp.Value = progress.growth.exp;
        MaxExp.Value = progress.growth.GetRequiredExp();
    }
    public void AddGold(int amount)
    {
        if (Data == null) 
            return;
        Data.progress.currency.AddGold(amount); 
        Gold.Value = Data.progress.currency.gold;
    }
    
     public void AddExp(int amount)
     {
        if (Data == null) 
            return;
        Data.progress.growth.AddExp(amount);
    
        // 레벨업 가능성이 있으므로 모든 성장 관련 값 동기화
        Level.Value = Data.progress.growth.level;
        Exp.Value = Data.progress.growth.exp;
        MaxExp.Value = Data.progress.growth.GetRequiredExp();
    }

    public SaveData LoadOrCreate(int slotIndex, int saveVersion = 1, bool saveImmediatelyIfCreated = false)
    {
        SaveData loaded = Load(slotIndex);
        if (loaded != null)
            return loaded;

        return CreateNew(slotIndex, saveVersion, saveImmediatelyIfCreated);
    }

    public bool Save()
    {
        return Save(CurrentSlotIndex);
    }

    public bool Save(int slotIndex)
    {
        if (Data == null)
            return false;
        
        SetCurrentSlot(slotIndex);
        
        Data.MarkSavedNowUtc();

        string fullPath = GetSaveFullPath(CurrentSlotIndex);
        EnsureDirectory(fullPath);

        string pathWithoutExt = GetSavePathWithoutExt(CurrentSlotIndex);
        JsonWriter.Save(Data, pathWithoutExt);
        
        OnSaved?.Invoke(CurrentSlotIndex, Data);
        
        Debug.Log($"[SaveDataManager] Save Success. (slot = {CurrentSlotIndex})");
        return true;
    }

    public bool Delete(int slotIndex)
    {
        string fullPath = GetSaveFullPath(slotIndex);
        if (File.Exists(fullPath) == false)
            return false;
        
        File.Delete(fullPath);

        if (slotIndex == CurrentSlotIndex)
        {
            Data = null;
            Gold.Value = 0;
            Level.Value = 1;
        }

        return true;
    }
    
    
    // ================================
    //           Path Utils
    // ================================

    private string GetSaveDir()
    {
        return Path.Combine(Application.persistentDataPath, saveFolderName);
    }

    private string GetSlotFileNameWithoutExt(int slotIndex)
    {
        return $"{baseFileNameWithoutExt}_slot{slotIndex}";
    }

    public string GetSavePathWithoutExt(int slotIndex)
    {
        string dir = GetSaveDir();
        return Path.Combine(dir, GetSlotFileNameWithoutExt(slotIndex));
    }

    public string GetSaveFullPath(int slotIndex)
    {
        return GetSavePathWithoutExt(slotIndex) + ".json";
    }

    private static void EnsureDirectory(string fullPath)
    {
        string dir = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(dir))
            return;
        
        if (Directory.Exists(dir) == false)
            Directory.CreateDirectory(dir);
    }

}