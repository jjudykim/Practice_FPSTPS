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
    [SerializeField] private KeyCode buildNewRandomMapKey = KeyCode.A;
    [SerializeField] private KeyCode saveKey = KeyCode.S;
    [SerializeField] private KeyCode loadRandomFromListKey = KeyCode.L;

    private MapPresetRepository repo;
    
    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();

        if (mapListManager == null)
            mapListManager = FindFirstObjectByType<MapListManager>();

        repo = new MapPresetRepository();
        repo.RebuildSeedCache();
    }

    private void Update()
    {
        if (Input.GetKeyDown(buildNewRandomMapKey))
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
    
    // =========================================================
    // MapList.json에 있는 목록 중 랜덤 1개로 맵 띄우기
    // =========================================================
    public void BuildRandomFromSeedCache()
    {
        if (CheckMapSystem() == false)
            return;

        if (repo.TryPickRandomSeed(out int seed) == false)
        {
            Debug.LogWarning("[MapSaveTestRunner] ::: Seed cache empty. No files in Maps folder?");
            return;
        }

        if (repo.TryLoadPresetBySeed(seed, out MapData data) == false)
        {
            Debug.LogError($"[MapSaveTestRunner] Preset load failed. seed={seed}");
            return;
        }
        
        mapSystem.Build(data.MapId, data.UsedSeed);
        Debug.Log($"[MapSaveTestRunner] BuildRandomFromSeedCache seed={seed}");
    }
    
    // =========================================================
    // Save + MapList 갱신
    // =========================================================
    private void SaveCurrentMapAndUpdateList()
    {
        if (CheckMapSystem() == false)
            return;

        if (mapSystem.CurrentMap == null)
        {
            Debug.LogWarning("[MapSaveTestRunner] ::: CurrentMap is null. Build first.");
            return;
        }
        
        int usedSeed = mapSystem.CurrentMap.UsedSeed;
        int currentMapId = mapSystem.CurrentMap.MapId;
        
        MapBuildRequestData reqData = MapBuildRequestData.FromMapSystemSettings(mapSystem, currentMapId, usedSeed);
        MapData data = MapData.Create(currentMapId, usedSeed, reqData);

        repo.SavePreset(data);

        string key = usedSeed.ToString();
        string presetPath = repo.GetPresetPath(usedSeed);
        repo.UpsertMapListEntry(key, presetPath);
        
        if (mapListManager != null)
            mapListManager.InvalidateCache();
        
        Debug.Log($"[MapSaveTestRunner] ::: Saved + MapList updated. key={key}, file={presetPath}");
    }
    
    // =========================================================
    // Utils
    // =========================================================
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