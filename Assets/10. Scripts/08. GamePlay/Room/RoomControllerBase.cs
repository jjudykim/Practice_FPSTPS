using UnityEngine;
using System;

public abstract class RoomControllerBase : MonoBehaviour
{
    public int NodeId { get; private set; } = -1;
    
    public event Action<RoomResult> OnRoomFinished;

    protected bool isFinished = false;
    
    public virtual void Init(int nodeId)
    {
        NodeId = nodeId;
        isFinished = false;
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