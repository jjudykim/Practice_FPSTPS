using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Random = System.Random;

public class MapRunEntryController : MonoBehaviour
{
    [Header("References (MapScene Only)")] 
    [SerializeField] private MapSystem mapSystem;
    [SerializeField] private MapUIController mapUI;

    [Header("UI Settings")] 
    [SerializeField] private bool openUIOnEnter = true;
    [SerializeField] private bool applyProgressOnOpen = true;

    private MapPresetRepository repo;

    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();
        
        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>();

        if (mapSystem != null)
            mapSystem.OnMapBuilt += HandleMapBuilt;
        
        repo = new MapPresetRepository();
        repo.RebuildSeedCache();
    }

    private void OnDestroy()
    {
        if (mapSystem != null)
            mapSystem.OnMapBuilt -= HandleMapBuilt;
    }

    private void Start()
    {
        if (TryOpenFromExistingCache())
            return;

        TryBuildRandomFromSavedPreset();
        
        Debug.LogWarning("[MapRunEntryController] ::: No cache, no preset, and fallback disabled. Nothing to start.");
    }

    
    private void HandleMapBuilt(MapContext context)
    {
        if (context == null || context.Graph == null)
            return;

        if (GameManager.Instance == null)
            return;
        
        GameManager.Instance.SetCurrentMap(context.Graph, context.UsedSeed);

        mapUI.Open(context.Graph, false);
        
        if (applyProgressOnOpen)
            mapUI.ApplyProgress(GameManager.Instance.MapCache, MapUIController.MapUIMode.Interactive);
        
        Debug.Log($"[MapRunEntryController] ::: Build completed. Cached & UI opened. seed={context.UsedSeed}");
    }
    
    private bool TryOpenFromExistingCache()
    {
        if (GameManager.Instance == null)
            return false;

        MapRunCache cache = GameManager.Instance.MapCache;
        
        if (cache == null || cache.HasGraph == false || cache.CurrentGraph == null)
            return false;

        if (openUIOnEnter == false)
            return true;

        if (mapUI == null)
            return true;
        
        mapUI.Open(cache.CurrentGraph, false);
        
        if (applyProgressOnOpen)
            mapUI.ApplyProgress(cache, MapUIController.MapUIMode.Interactive);
        
        Debug.Log($"[MapRunEntryController] ::: Opened UI from cache. seed={cache.CurrentSeed}, current={cache.CurrentNodeId}, cleared={cache.ClearedNodeIds.Count}");
        return true;
    }

    private bool TryBuildRandomFromSavedPreset()
    {
        if (mapSystem == null)
            return false;

        if (repo == null)
            return false;

        if (repo.TryPickRandomSeed(out int seed) == false)
            return false;

        if (repo.TryLoadPresetBySeed(seed, out MapData data) == false)
            return false;
        
        mapSystem.Build(data.MapId, data.UsedSeed);
        return true;
    }
}


