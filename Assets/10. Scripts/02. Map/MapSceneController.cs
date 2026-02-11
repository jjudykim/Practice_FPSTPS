using UnityEngine;

public class MapSceneController : MonoBehaviour
{
    [Header("References (Map Scene Only)")]
    [SerializeField] private MapUIController mapUI;
    
    [Header("UI Settings")]
    [SerializeField] private bool openUIOnEnter = true;
    [SerializeField] private bool applyProgressOnOpen = true;

    private void Awake()
    {
        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>(FindObjectsInactive.Include);
    }
    
    private void OnDestroy()
    {
        if (TryOpenFromExistingCache())
        {
            Debug.Log("[MapSceneController] ::: Successfully initialized MapScene from GameManager cache.");
            return;
        }
        
        //  오류 상황
        Debug.LogWarning("[MapSceneController] ::: No existing map cache found. Redirecting to Lobby or building default.");
    }
    
    private void Start()
    {
        if (TryOpenFromExistingCache())
            return;
        
        Debug.LogWarning("[MapSceneController] ::: No existing cache. Skip auto build");
    }

    private bool TryOpenFromExistingCache()
    {
        if (Managers.Instance.Game == null)
            return false;

        MapRunCache cache = Managers.Instance.Game.MapCache;
        if (cache == null || cache.HasGraph == false || cache.CurrentGraph == null)
            return false;

        if (openUIOnEnter == false)
            return true;
        
        mapUI.Open(cache.CurrentGraph, false);

        if (applyProgressOnOpen)
        {
            mapUI.ApplyProgress(cache,  MapUIController.MapUIMode.Interactive);
        
            if (cache.CurrentNodeId < 0) 
                cache.TrySetCurrentNodeToStartIfNeeded();
            
            if (cache.CurrentNodeId >= 0)
                mapUI.SnapToNode(cache.CurrentNodeId);
        }
        
        return true;
    }
}
