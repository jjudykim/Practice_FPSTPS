using UnityEditor.Rendering;

public class MapContext
{
    public int MapId { get; }
    public int Seed { get; }
    public MapGraph Graph { get; }

    public MapContext(int mapId, int seed, MapGraph graph)
    {
        MapId = mapId;
        Seed = seed;
        Graph = graph;
    }
}