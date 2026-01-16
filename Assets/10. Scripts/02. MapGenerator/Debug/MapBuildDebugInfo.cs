using System;
using System.Collections.Generic;

public class MapBuildDebugInfo
{
    public int MapId { get; }
    public int Seed { get; }
    public int TotalDepth { get; }

    public Dictionary<int, List<int>> DepthNodes { get; } = new Dictionary<int, List<int>>();
    public List<EdgeBuildRecord> MainPathEdges { get; } = new List<EdgeBuildRecord>();
    public List<EdgeBuildRecord> ExtraEdges { get; } = new List<EdgeBuildRecord>();

    public int ExtraBudgetTotal { get; private set; }
    public int ExtraBudgetNormal { get; private set; }
    public int ExtraBudgetSkip { get; private set; }
    
    [Serializable]
    public class EdgeBuildRecord
    {
        public int FromId;
        public int ToId;

        public int FromDepth;
        public int ToDepth;

        public float Score;

        public int LaneDistance;
        public int IncomingBefore;

        public EdgeBuildStage Stage;
    }

    public enum EdgeBuildStage
    {
        MainPath,
        ExtraEdge_Normal,
        ExtraEdge_Skip,
    }

    public MapBuildDebugInfo(int mapId, int seed, int totalDepth)
    {
        MapId = mapId;
        Seed = seed;
        TotalDepth = totalDepth;
    }
    
    public void AddDepthNodeList(int depth, List<int> nodeIds)
    {
        DepthNodes[depth] = nodeIds;
    }

    public void AddMainPathEdge(int fromId, int toId, int depth)
    {
        MainPathEdges.Add(new EdgeBuildRecord
        {
            FromId = fromId,
            ToId = toId,
            FromDepth = depth - 1,
            ToDepth = depth,
            Score = 0f,
            Stage = EdgeBuildStage.MainPath
        });
    }

    public void AddExtraEdge(int fromId, int toId, int fromDepth, int toDepth, float score, int laneDistance, int incomingBefore, EdgeBuildStage stage)
    {
        ExtraEdges.Add(new EdgeBuildRecord
        {
            FromId = fromId,
            ToId = toId,
            FromDepth = fromDepth,
            ToDepth = toDepth,
            Score = score,
            LaneDistance = laneDistance,
            IncomingBefore = incomingBefore,
            Stage = stage
        });
    }

    public void SetExtraBudget(int total, int normal, int skip)
    {
        ExtraBudgetTotal = total;
        ExtraBudgetNormal = normal;
        ExtraBudgetSkip = skip;
    }
}