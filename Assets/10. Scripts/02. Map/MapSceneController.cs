using UnityEngine;

public class MapSceneController : MonoBehaviour
{
    [Header("References (Map Scene Only)")]
    [SerializeField] private MapSystem mapSystem;
    [SerializeField] private MapUIController mapUI;
    
    [Header("UI Settings")]
    [SerializeField] private bool openUIOnEnter = true;
    [SerializeField] private bool applyProgressOnOpen = true;

    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();
        
        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>(FindObjectsInactive.Include);

        if (mapSystem != null)
            mapSystem.OnMapBuilt += HandleMapBuilt;
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
        
        Debug.LogWarning("[MapSceneController] ::: No existing cache. Skip auto build");
    }

    private void BuildNewSession()
    {
        if (mapSystem == null)
        {
            Debug.LogError("[MapSceneController] BuildNewSession failed: mapSystem is null");
            return;
        }

        int seed = Random.Range(0, int.MaxValue);
        mapSystem.Build(1, seed);

        Debug.Log($"[MapSceneController] BuildNewSession requested. seed={seed}");
    }

    private bool TryOpenFromExistingCache()
    {
        if (Managers.Instance.Game == null)
        {
            Debug.LogWarning("[MapSceneController] ::: GameManager.Instance is null");
            return false;
        }

        MapRunCache cache = Managers.Instance.Game.MapCache;
        if (cache == null || cache.HasGraph == false || cache.CurrentGraph == null)
            return false;

        if (openUIOnEnter == false)
            return true;

        if (mapUI == null)
        {
            Debug.LogError("[MapSceneController] ::: mapUI is null");
            return true;
        }
        
        mapUI.Open(cache.CurrentGraph, false);

        if (applyProgressOnOpen)
            mapUI.ApplyProgress(cache,  MapUIController.MapUIMode.Interactive);

        Debug.Log($"[MapSceneController] ::: Opened UI from existing cache. current={cache.CurrentNodeId}, cleared={cache.ClearedNodeIds.Count}");
        return true;
    }
    
    private void HandleMapBuilt(MapContext ctx)
    {
        if (ctx == null || ctx.Graph == null)
        {
            Debug.LogError("[MapSceneController] ::: HandleMapBuilt failed: ctx/graph is null");
            return;
        }

        if (Managers.Instance.Game == null)
        {
            Debug.LogError("[MapSceneController] ::: HandleMapBuilt failed: GameManager.Instance is null");
            return;
        }
        
        Managers.Instance.Game.SetCurrentMap(ctx.Graph, ctx.UsedSeed);
        
        if (openUIOnEnter == false || mapUI == null)
            return;
        
        mapUI.Open(ctx.Graph, false);

        if (applyProgressOnOpen)
            mapUI.ApplyProgress(Managers.Instance.Game.MapCache,  MapUIController.MapUIMode.Interactive);

        Debug.Log($"[MapSceneController] ::: Build completed. Cached & UI opened. seed={ctx.UsedSeed}");
    }
}
