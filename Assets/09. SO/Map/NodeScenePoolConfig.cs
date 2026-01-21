using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Node Scene Pool Config", fileName = "NodeScenePoolConfig")]
public class NodeScenePoolConfig : ScriptableObject
{
    [Serializable]
    public class NodeScenePool
    {
        public NodeType nodeType;
        public List<string> sceneNames = new List<string>();
    }

    [SerializeField] private List<NodeScenePool> pools = new List<NodeScenePool>();

    public IReadOnlyList<string> GetCandidates(NodeType type)
    {
        for (int i = 0; i < pools.Count; ++i)
        {
            if (pools[i].nodeType == type)
                return pools[i].sceneNames;
        }

        return Array.Empty<string>();
    }
}
