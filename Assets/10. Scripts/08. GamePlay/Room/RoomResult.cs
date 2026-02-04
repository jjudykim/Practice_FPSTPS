public enum RoomResultType
{
    Cleared,
    Failed,
    Aborted
}

public struct RoomResult
{
    public RoomResultType Type;
    public int NodeId;              // 어떤 노드(방)였는지
    
    public float ClearTime;         // 클리어 타임
    public int KillCount;           // 킬 수
    
    public string RewardKey;        // 보상 테이블 키 등

    public static RoomResult Cleared(int nodeId, float clearTime = 0f, int kills = 0, string rewardKey = null)
    {
        return new RoomResult
        {
            Type = RoomResultType.Cleared,
            NodeId = nodeId,
            ClearTime = clearTime,
            KillCount = kills,
            RewardKey = rewardKey
        };
    }

    public static RoomResult Failed(int nodeId)
    {
        return new RoomResult
        {
            Type = RoomResultType.Failed,
            NodeId = nodeId
        };
    }

    public static RoomResult Aborted(int nodeId)
    {
        return new RoomResult
        {
            Type = RoomResultType.Aborted,
            NodeId = nodeId
        };
    }
}