using System;
using System.IO;
using UnityEngine;

public class SaveDataManager
{
    private const string DefaultSaveFolderName = "Save";
    private const string DefaultBaseFileName = "saveData";

    [SerializeField] private string saveFolderName = DefaultSaveFolderName;
    [SerializeField] private string baseFileNameWithoutExt = DefaultBaseFileName;

    public int CurrentSlotIndex { get; private set; } = 0;
    public SaveData Data { get; private set; }
    
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
        OnLoaded?.Invoke(CurrentSlotIndex, Data);
        return Data;
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
            Data = null;

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