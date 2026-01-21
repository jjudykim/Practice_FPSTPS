using UnityEngine;

public class MapOverlayPanel : MonoBehaviour
{
    [SerializeField] private MapUIController mapUIController;

    private void Awake()
    {
        if (mapUIController == null)
            mapUIController = GetComponentInChildren<MapUIController>(true);
    }

    public void Open(MapGraph graph, bool viewOnly = true)
    {
        if (graph == null)
        {
            Debug.LogError("[MapOverlayPanel] ::: Open failed: graph is null");
            return;
        }
        
        if (mapUIController == null)
        {
            Debug.LogError("[MapOverlayPanel] ::: MapUIController is null");
            return;
        }

        mapUIController.Open(graph);
    }

    public void ApplyProgress(MapRunCache cache, bool viewOnly)
    {
        if (cache == null || cache.HasGraph == false)
        {
            Debug.LogWarning("[MapOverlayPanel] ::: ApplyProgress failed: cache is empty");
            return;
        }

        if (mapUIController == null)
            return;
        
        if (viewOnly)
            mapUIController.ApplyProgress(cache, MapUIController.MapUIMode.ViewOnly);
        else
            mapUIController.ApplyProgress(cache, MapUIController.MapUIMode.Interactive);
    }
}