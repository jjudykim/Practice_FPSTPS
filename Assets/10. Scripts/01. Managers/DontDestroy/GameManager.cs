using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameState
{
    Ready,
    Playing,
    GameOver,
    GameClear
}

public class GameManager
{
    public GameState CurrentState { get; private set; } = GameState.Ready;
    public SessionContext Session { get; private set; } = new SessionContext();
    public event Action<GameState> OnStateChanged;
    private MapPresetRepository mapRepo = new MapPresetRepository();
    private GameResultUI gameResultUI;

    public void GameOver() => Managers.Instance.StartCoroutine(CoHandleGameFinish(GameState.GameOver));
    public void GameClear() => Managers.Instance.StartCoroutine(CoHandleGameFinish(GameState.GameClear));
    
    public MapRunCache MapCache { get; private set; } = new MapRunCache();
    public bool HasActiveMap => MapCache != null && MapCache.HasGraph;
    
    private readonly string mapSceneName = "MapScene";

    public void Init()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/UI/GameResultUI");
        if (prefab != null)
        {
            var go = UnityEngine.Object.Instantiate(prefab, Managers.Instance.transform);
            gameResultUI = go.GetComponent<GameResultUI>();
        }
    }

    public void Awake()
    {
        if (MapCache == null)
            MapCache = new MapRunCache();
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        
        if (BuildNewRunMap())
        {
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log("[GameManager] ::: Game Started and Map Build Success");
        }
        else
            Debug.Log("[GameManager] ::: Failed to start game : Map Build Failed.");
    }

    private bool BuildNewRunMap()
    {
        if (mapRepo.TryPickRandomSeed(out int selectedSeed))
        {
            if (mapRepo.TryLoadPresetBySeed(selectedSeed, out MapData data))
            {
                var graph = MapGraphGenerator.GenerateFromData(data);
                if (graph != null)
                {
                    SetCurrentMap(graph, selectedSeed, graph.StartNode.Id);
                    return true;
                }
            }
        }
        return false;
    }

    public void SetCurrentMap(MapGraph graph, int usedSeed, int startNodeId = -1)
    {
        if (graph == null)
            return;
        
        MapCache.SetGraph(graph, usedSeed);

        if (startNodeId != -1)
            MapCache.SetCurrentNode(startNodeId);
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
            
            Session.AddKill();
            
            if (string.IsNullOrEmpty(mapSceneName))
                return;
            
            Managers.Instance.Scene.LoadScene(mapSceneName);
            
            // 보상/업적/해금 트리거는 "런 결과 파이프라인"에서 처리 (여기서 하거나 별도 시스템으로 위임)
            // TODO: ApplyRoomRewards(result);
            return;
        }

        // 실패/중단이면 세션 종료 처리로 연결 가능
        if (result.Type == RoomResultType.Failed)
        {
            Debug.Log($"[GameManager] Room ended as {result.Type}. End Run. nodeId={result.NodeId}");
            // TODO: 런 종료(귀환/사망) 처리, 보상/패널티, Base로 복귀 등
            Session.Inventory.Clear();
            return;
        }
        
        if (result.Type == RoomResultType.Aborted)
        {
            Debug.Log($"[GameManager] Room Aborted. nodeId={result.NodeId}");
            // 중단 처리
            Session.Inventory.Clear();
            return;
        }
    }

    public void FinalizeSessionSuccess()
    {
        var storage = Managers.Instance.SaveData.Data.progress.inventory;
        //Session.Inventory(storage);

        Managers.Instance.SaveData.Save();
    }

    private IEnumerator CoHandleGameFinish(GameState targetState)
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.GameClear)
            yield break;

        CurrentState = targetState;
        
        yield return new WaitForSeconds(2.0f);
        
        OnStateChanged?.Invoke(CurrentState);
        
        if (Managers.Instance.Scene != null)
        {
            yield return Managers.Instance.StartCoroutine(Managers.Instance.Scene.CoFadeOut(1.0f));
        }
    }

    public void ResetToDefault()
    {
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Player.Instance != null)
        {
            Player.Instance.ResetForTown();
        }

        if (GlobalHUDUI.Instance != null)
        {
            GlobalHUDUI.Instance.RefreshAll();
        }
    }

    public void ResetGameSession()
    {
        CurrentState = GameState.Ready;
    }
}