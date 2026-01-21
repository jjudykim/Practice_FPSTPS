using System.Collections.Generic;
using System.Linq;

public class MapGraph
{
    private readonly List<MapNode> nodes;

    public IReadOnlyList<MapNode> Nodes => nodes;
    public MapGraph(List<MapNode> nodes) => this.nodes = nodes;
    
    public MapNode StartNode => nodes.First(n => n.Type == NodeType.Start);
    public MapNode BossNode => nodes.First(n => n.Type == NodeType.Boss);
    
    public List<MapNode> GetNodesAtDepth(int depth) => nodes.Where(n => n.Depth == depth).ToList();
    public MapNode GetNodeById(int id) => nodes.FirstOrDefault(n => n.Id == id);

    public int GetIncomingCount(int targetId) => nodes.Count(n => n.NextNodeIds != null && n.NextNodeIds.Contains(targetId)); 
}