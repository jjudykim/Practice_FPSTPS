using System;
using System.Collections.Generic;

[Serializable]
public class MapRunCache
{
    public MapGraph CurrentGraph { get; private set; }
    public int CurrentSeed { get; private set; } = 0;
    public int CurrentNodeId { get; private set; } = -1;

    public HashSet<int> ClearedNodeIds { get; } = new();

    [Serializable]
    public struct VisitedEdge
    {
        public int From;
        public int To;

        public VisitedEdge(int from, int to)
        {
            From = from;
            To = to;
        }

        public bool Equals(VisitedEdge other) => From == other.From && To == other.To;
        public override bool Equals(object obj) => obj is VisitedEdge other && Equals(other);
        public override string ToString() => $"{From}->{To}";
    }
    
    public List<VisitedEdge> VisitedEdges { get; } = new();

    public bool HasGraph => CurrentGraph != null;

    public void SetGraph(MapGraph graph, int seed)
    {
        CurrentGraph = graph;
        CurrentSeed = seed;
            
        CurrentNodeId = -1;
        
        ClearedNodeIds.Clear();
        VisitedEdges.Clear();
    }

    public void SetCurrentNode(int nodeId)
    {
        CurrentNodeId = nodeId;
    }

    public void TrySetCurrentNodeToStartIfNeeded()
    {
        if (CurrentGraph == null)
            return;

        if (CurrentNodeId >= 0)
            return;

        for (int i = 0; i < CurrentGraph.Nodes.Count; ++i)
        {
            MapNode n = CurrentGraph.Nodes[i];
            if (n.Type == NodeType.Start)
            {
                CurrentNodeId = n.Id;
                return;
            }
        }

        if (CurrentGraph.Nodes.Count > 0)
            CurrentNodeId = CurrentGraph.Nodes[0].Id;
    }

    public void MarkCleared(int nodeId)
    {
        if (nodeId < 0)
            return;
        
        ClearedNodeIds.Add(nodeId);
    }

    public void RecordVisitedEdge(int fromNodeId, int toNodeId, bool preventDuplicate = true)
    {
        if (fromNodeId < 0 || toNodeId < 0)
            return;

        VisitedEdge e = new VisitedEdge(fromNodeId, toNodeId);

        if (preventDuplicate)
        {
            if (VisitedEdges.Contains(e))
                return;
        }

        VisitedEdges.Add(e);
    }
 
    public bool IsVisitedEdge(int fromNodeId, int toNodeId) => VisitedEdges.Contains(new VisitedEdge(fromNodeId, toNodeId));

    public void Clear()
    {
        CurrentGraph = null;
        CurrentSeed = 0;
        CurrentNodeId = -1;
        
        ClearedNodeIds.Clear();
        VisitedEdges.Clear();
    }
}