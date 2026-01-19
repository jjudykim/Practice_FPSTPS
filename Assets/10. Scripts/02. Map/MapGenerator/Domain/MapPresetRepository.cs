using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class MapPresetRepository
{
    private const string FolderName = "Maps";
    private const string MapListFileName = "MapList.json";

    private readonly HashSet<int> cachedSeeds = new HashSet<int>();
    private bool isCached = false;
    
    // Paths
    public string FolderPath => Path.Combine(Application.persistentDataPath, FolderName);
    public string MapListPath => Path.Combine(Application.persistentDataPath, MapListFileName);
    public string GetPresetPath(int seed) => Path.Combine(FolderPath, $"{seed}.json");
    
    // Cache
    public void EnsureFolder()
    {
        if (Directory.Exists(FolderPath) == false)
            Directory.CreateDirectory(FolderPath);
    }

    public void RebuildSeedCache()
    {
        EnsureFolder();

        cachedSeeds.Clear();

        string[] files = Directory.GetFiles(FolderPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            
            if (int.TryParse(fileName, out int seed))
                cachedSeeds.Add(seed);
        }

        isCached = true;
        Debug.Log($"[MapPresetRepository] ::: Seed cache rebuilt. Count={cachedSeeds.Count}");
    }

    public IReadOnlyCollection<int> GetSeeds()
    {
        if (isCached == false)
            RebuildSeedCache();

        return cachedSeeds;
    }

    public bool TryPickRandomSeed(out int seed)
    {
        if (isCached == false)
            RebuildSeedCache();

        if (cachedSeeds.Count == 0)
        {
            seed = 0;
            return false;
        }

        int[] arr = cachedSeeds.ToArray();
        seed = arr[UnityEngine.Random.Range(0, arr.Length)];
        return true;
    }
    
    public void AddSeedToCache(int seed)
    {
        if (isCached == false)
            RebuildSeedCache();
        
        cachedSeeds.Add(seed);
    }
    
    // Save / Load MapData
    public void SavePreset(MapData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        
        EnsureFolder();

        string path = GetPresetPath(data.UsedSeed);
        JsonWriter.Save(data, path);
        
        AddSeedToCache(data.UsedSeed);
    }

    public bool TryLoadPresetBySeed(int seed, out MapData data)
    {
        string path = GetPresetPath(seed);
        data = JsonReader.Load<MapData>(path);

        return data != default;
    }
    
    // MapList.json
    public MapListData LoadOrCreateMapList()
    {
        if (File.Exists(MapListPath) == false)
        {
            var created = new MapListData();
            JsonWriter.Save(created, MapListPath);
            
            return created;
        }
        
        var loaded = JsonReader.Load<MapListData>(MapListPath);
        if (loaded == default || loaded.entries == null)
        {
            var created = new MapListData();
            JsonWriter.Save(created, MapListPath);
            return created;
        }

        return loaded;
    }

    public void UpsertMapListEntry(string key, string presetPath)
    {
        MapListData list = LoadOrCreateMapList();
        
        MapListEntry entry = list.entries.FirstOrDefault(x => x.key == key);
        if (entry == null)
        {
            entry = new MapListEntry { key = key, presetFile = presetPath };
            list.entries.Add(entry);
        }
        else
        {
            entry.presetFile = presetPath;
        }
        
        JsonWriter.Save(list, MapListPath);
    }
}