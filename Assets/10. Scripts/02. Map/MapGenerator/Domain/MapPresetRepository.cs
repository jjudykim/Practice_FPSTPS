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

        // 1. Streaming 확인
        string streamingDir = Path.Combine(Application.streamingAssetsPath, FolderName);
        if (Directory.Exists(streamingDir))
        {
            foreach (string file in Directory.GetFiles(streamingDir, "*.json"))
            {
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out int seed))
                    cachedSeeds.Add(seed);
            }
        }

        // 2. Persistent 확인 (덮어쓰기 효과)
        if (Directory.Exists(FolderPath))
        {
            foreach (string file in Directory.GetFiles(FolderPath, "*.json"))
            {
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out int seed))
                    cachedSeeds.Add(seed);
            }
        }

        isCached = true;
        Debug.Log($"[MapPresetRepository] ::: Seed cache rebuilt (Streaming+Persistent). Count={cachedSeeds.Count}");
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
        // 1. Persistent 확인
        string path = GetPresetPath(seed);
        if (File.Exists(path))
        {
            data = JsonReader.Load<MapData>(path);
            if (data != default) return true;
        }

        // 2. Streaming 확인
        path = Path.Combine(Application.streamingAssetsPath, FolderName, $"{seed}.json");
        if (File.Exists(path))
        {
            data = JsonReader.Load<MapData>(path);
            if (data != default) return true;
        }

        data = default;
        return false;
    }
    
    // MapList.json
    public MapListData LoadOrCreateMapList()
    {
        // 1. Persistent 확인
        if (File.Exists(MapListPath))
        {
            var loaded = JsonReader.Load<MapListData>(MapListPath);
            if (loaded != default && loaded.entries != null) return loaded;
        }

        // 2. Streaming 확인
        string sPath = Path.Combine(Application.streamingAssetsPath, MapListFileName);
        if (File.Exists(sPath))
        {
            var loaded = JsonReader.Load<MapListData>(sPath);
            if (loaded != default && loaded.entries != null) return loaded;
        }
        
        // 3. 새로 생성
        var created = new MapListData();
        JsonWriter.Save(created, MapListPath);
        return created;
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
