using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager
{
    public MapRunCache MapCache { get; private set; } = new MapRunCache();
    public bool HasActiveMap => MapCache != null && MapCache.HasGraph;
    
    private readonly string mapSceneName = "MapScene";
    
    public void Awake()
    {
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

    public void OnRoomFinished(RoomResult result)
    {
        if (!HasActiveMap)
        {
            Debug.LogWarning("[GameManager] OnRoomFinished ignored: no active map.");
            return;
        }
        
        if (result.Type == RoomResultType.Cleared)
        {
            Debug.Log($"[GameManager] Room Cleared. nodeId={result.NodeId}, time={result.ClearTime}, kills={result.KillCount}");
            
            // TODO: OpenMapUI();
            if (string.IsNullOrEmpty(mapSceneName))
                return;
        
            SceneManager.LoadScene(mapSceneName);
            
            // 보상/업적/해금 트리거는 "런 결과 파이프라인"에서 처리 (여기서 하거나 별도 시스템으로 위임)
            // TODO: ApplyRoomRewards(result);
            return;
        }

        // 실패/중단이면 세션 종료 처리로 연결 가능
        if (result.Type == RoomResultType.Failed)
        {
            Debug.Log($"[GameManager] Room ended as {result.Type}. End Run. nodeId={result.NodeId}");
            // TODO: 런 종료(귀환/사망) 처리, 보상/패널티, Base로 복귀 등
            return;
        }
        
        if (result.Type == RoomResultType.Aborted)
        {
            Debug.Log($"[GameManager] Room Aborted. nodeId={result.NodeId}");
            // 중단 처리
            return;
        }
        
        

        
    }
}