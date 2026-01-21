using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : SingletonBase<GameManager>
{
    public MapRunCache MapCache { get; private set; } = new MapRunCache();
    public bool HasActiveMap => MapCache != null && MapCache.HasGraph;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (MapCache == null)
            MapCache = new MapRunCache();
    }

    public void SetCurrentMap(MapGraph graph, int usedSeed, int startNodeId = -1)
    {
        if (graph == null)
        {
            Debug.LogError("[GameManager] ::: SetCurrentMap failed: graph is null");
            return;
        }
        
        MapCache.SetGraph(graph, usedSeed);

        //if (startNodeId >= 0)
        //    MapCache.SetCurrentNode(startNodeId);
        //else
        //    MapCache.TrySetCurrentNodeToStartIfNeeded();
        MapCache.SetCurrentNode(startNodeId);
        
        Debug.Log($"[GameManager] ::: CurrentMap cached. seed={usedSeed}, currentNodeId={MapCache.CurrentNodeId}");
    }

    public void SelectNextNodeAndMove(int toNodeId)
    {
        int fromNodeId = MapCache.CurrentNodeId;

        if (fromNodeId >= 0 && fromNodeId != toNodeId)
        {
            MapCache.RecordVisitedEdge(fromNodeId, toNodeId);
            MapCache.RecordVisitedEdge(toNodeId, fromNodeId);
        }

        MapCache.SetCurrentNode(toNodeId);
    }

    public void ClearCurrentMap()
    {
        MapCache.Clear();
        Debug.Log("[GameManager] ::: CurrentMap cache cleared.");
    }
}