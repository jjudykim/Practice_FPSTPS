using UnityEngine;
using System;

public abstract class RoomControllerBase : MonoBehaviour
{
    [SerializeField] private MapUIController mapUI;
    
    public int NodeId { get; private set; } = -1;
    
    public event Action<RoomResult> OnRoomFinished;

    protected bool isFinished = false;
    
    public virtual void Init(int nodeId)
    {
        NodeId = nodeId;
        isFinished = false;

        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>();
    }

    protected virtual void Update()
    {
        if (Managers.Instance.Input.SessionMap)
            HandleMapToggle();
    }

    private void HandleMapToggle()
    {
        if (mapUI == null)
            return;

        var mapCache = Managers.Instance.Game.MapCache;
        if (mapCache == null || mapCache.CurrentGraph == null)
        {
            Debug.LogWarning("[RoomControllerBase] ::: 표시할 맵 데이터가 없습니다.");
            return;
        }
        
        mapUI.Toggle(mapCache.CurrentGraph, MapUIController.MapUIMode.ViewOnly);

        if (mapUI.IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mapUI.ApplyProgress(mapCache, MapUIController.MapUIMode.ViewOnly);
            
            if (mapCache.CurrentNodeId >= 0)
                mapUI.SnapToNode(mapCache.CurrentNodeId);
        }
        else
        {
            CameraController.Instance.ApplyCurModeImmediate();
        }
    }

    protected void FinishCleared(float clearTime = 0f, int kills = 0, string rewardKey = null)
    {
        if (isFinished) return;
        isFinished = true;

        RoomResult result = RoomResult.Cleared(NodeId, clearTime, kills, rewardKey);
        Debug.Log($"[RoomControllerBase] Room Cleared. nodeId={NodeId}");

        OnRoomFinished?.Invoke(result);
    }

    // 실패 조건(플레이어 사망 등)
    protected void FinishFailed()
    {
        if (isFinished) return;
        isFinished = true;

        RoomResult result = RoomResult.Failed(NodeId);
        Debug.Log($"[RoomControllerBase] Room Failed. nodeId={NodeId}");

        OnRoomFinished?.Invoke(result);
    }

    // 중단(메뉴로 나가기 등)
    protected void FinishAborted()
    {
        if (isFinished) return;
        isFinished = true;

        RoomResult result = RoomResult.Aborted(NodeId);
        Debug.Log($"[RoomControllerBase] Room Aborted. nodeId={NodeId}");

        OnRoomFinished?.Invoke(result);
    }
}