using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start,
    Combat,
    Reward,
    Shop,
    Boss,
}

public class MapNode
{
    public int Id { get; }
    public int Depth { get; }
    public NodeType Type { get; set; }

    public List<int> NextNodeIds { get; } = new List<int>();

    public MapNode(int id, int depth, NodeType type)
    {
        Id = id;
        Depth = depth;
        Type = type;
    }
}
