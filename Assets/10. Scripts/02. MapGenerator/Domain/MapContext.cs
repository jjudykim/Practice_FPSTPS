using UnityEditor.Rendering;

public class MapContext
{
    public int MapId { get; }
    public int BaseSeed { get; }    // 사용자가 요청한 원본 seed
    public int UsedSeed { get; }    // 실제로 InternalBuild에 들어가서 성공한 seed
    public MapGraph Graph { get; }

    public MapContext(int mapId, int baseSeed, int usedSeed, MapGraph graph)
    {
        MapId = mapId;
        BaseSeed = baseSeed;
        UsedSeed = usedSeed;
        Graph = graph;
    }
}