using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MapSaveTestRunner : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private MapSystem mapSystem;
    [SerializeField] private MapListManager mapListManager;
    
    [Header("Test Settings")] 
    [SerializeField] private int mapSeedForTest = 1;

    [Header("Debug HotKeys (Runner Owned")]
    [SerializeField] private KeyCode saveKey = KeyCode.S;
    [SerializeField] private KeyCode loadRandomFromListKey = KeyCode.L;
    
    private const string FolderName = "Maps";
    private const string MapListFileName = "MapList.json";
    
    private static string GetFolderPath() => Path.Combine(Application.persistentDataPath, FolderName);
    private static string GetFilePath(int seed) =>  Path.Combine(GetFolderPath(), $"{seed}.json");
    private static string GetMapListFilePath() => Path.Combine(Application.persistentDataPath, MapListFileName);

    private readonly HashSet<int> cachedSeeds = new HashSet<int>();
    private bool seedsCached = false;
    
    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();

        if (mapListManager == null)
            mapListManager = FindFirstObjectByType<MapListManager>();

        EnsureFolder();

        RebuildSeedCacheFromMapsFolder();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            RequestBuildNewRandomMap();

        if (Input.GetKeyDown(saveKey))
            SaveCurrentMapAndUpdateList();

        if (Input.GetKeyDown(loadRandomFromListKey))
            BuildRandomFromSeedCache();
    }

    // 랜덤 seed로 새 맵 생성 (MapSystem.Build 호출)
    public void RequestBuildNewRandomMap()
    {
        if (CheckMapSystem() == false)
            return;
        
        int randomSeed = Random.Range(0, int.MaxValue);
        mapSystem.Build(mapSeedForTest, randomSeed);
        
        Debug.Log($"[MapSaveTest] Build New Map. mapId={mapSeedForTest}, seed={randomSeed}");
    }
    
    // 현재 맵 저장
    private void SaveCurrentMapAndUpdateList()
    {
        if (CheckMapSystem() == false)
            return;

        if (mapSystem.CurrentMap == null)
        {
            Debug.LogWarning("[MapSaveTestRunner] ::: CurrentMap is null. Build first.");
            return;
        }

        EnsureFolder();
        EnsureSeedCacheReady();

        int usedSeed = mapSystem.CurrentMap.UsedSeed;
        int currentMapId = mapSystem.CurrentMap.MapId;
        
        MapBuildRequestData reqData = MapBuildRequestData.FromMapSystemSettings(mapSystem, currentMapId, usedSeed);
        MapData data = MapData.Create(currentMapId, usedSeed, reqData);

        string presetPath = GetFilePath(usedSeed);
        JsonWriter.Save(data, presetPath);

        cachedSeeds.Add(usedSeed);

        string key = usedSeed.ToString();
        UpdateMapListFile(key, presetPath);

        if (mapListManager != null)
            mapListManager.InvalidateCache();
    }

    // =========================================================
    // MapList.json에 있는 목록 중 랜덤 1개로 맵 띄우기
    // =========================================================
    private void BuildRandomFromMapList()
    {
        if (CheckMapSystem() == false)
            return;
        
        EnsureSeedCacheReady();

        if (mapListManager == null)
        {
            Debug.LogError("[MapSaveTestRunner] ::: MapListManager is null.");
            return;
        }
        
        mapSystem.BuildFromMapList(0);
        Debug.Log("[MapSaveTestRunner] ::: BuildRandomFromMapList requested");
    }
    
    public void BuildRandomFromSeedCache()
    {
        if (CheckMapSystem() == false)
            return;
        
        EnsureSeedCacheReady();

        if (cachedSeeds == null || cachedSeeds.Count == 0)
        {
            Debug.LogWarning("[MapSaveTestRunner] ::: Seed cache empty. No files in Maps folder?");
            return;
        }

        int[] seeds = cachedSeeds.ToArray();
        int pick = Random.Range(0, seeds.Length);
        int seed = seeds[pick];

        string path = GetFilePath(seed);
        MapData data = JsonReader.Load<MapData>(path);
        
        if (data == default)
        {
            Debug.LogError($"[MapSaveTestRunner] ::: Preset load failed. path={path}");
            return;
        }

        mapSystem.Build(data.MapId, data.UsedSeed);
        Debug.Log($"[MapSaveTestRunner] ::: BuildRandomFromSeedCache pick={pick}/{seeds.Length}, seed={seed}, path={path}");
    }

    private void EnsureSeedCacheReady()
    {
        if (seedsCached)
            return;
        
        RebuildSeedCacheFromMapsFolder();
    }
    
    private void RebuildSeedCacheFromMapsFolder()
    {
        cachedSeeds.Clear();

        string folder = GetFolderPath();
        if (Directory.Exists(folder) == false)
        {
            seedsCached = true;
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            if (int.TryParse(fileName, out int seed))
            {
                cachedSeeds.Add(seed);
            }
        }

        seedsCached = true;
        Debug.Log($"[MapSaveTestRunner] ::: SeedCache rebuilt. Count={cachedSeeds.Count}");
    }

    private void EnsureFolder()
    {
        string folder = GetFolderPath();
        if (Directory.Exists(folder) == false)
            Directory.CreateDirectory(folder);
    }

    private void UpdateMapListFile(string key, string presetpath)
    {
        MapListData list = LoadOrCreateMapList();
        
        MapListEntry entry = list.entries.FirstOrDefault(e => e != null && e.key == key);
        if (entry == null)
        {
            entry = new MapListEntry { key = key, presetFile = presetpath };
            list.entries.Add(entry);
        }
        else
        {
            entry.presetFile = presetpath;
        }
        
        JsonWriter.Save(list, GetMapListFilePath());
    }

    private MapListData LoadOrCreateMapList()
    {
        string path = GetMapListFilePath();
        MapListData list = JsonReader.Load<MapListData>(path);

        if (list == default || list.entries == null)
        {
            list = new MapListData
            {
                version = 1,
                entries = new List<MapListEntry>()
            };
            
            JsonWriter.Save(list, path);
        }

        return list;
    }

    private bool CheckMapSystem()
    {
        if (mapSystem == null)
        {
            Debug.LogError("[MapSaveTest] MapSystem Ref is null");
            return false;
        }

        return true;
    }
}