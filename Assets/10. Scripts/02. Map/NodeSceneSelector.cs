using System.Collections.Generic;

public class NodeSceneSelector
{
    private readonly NodeScenePoolConfig config;

    public NodeSceneSelector(NodeScenePoolConfig config)
    {
        this.config = config;
    }

    public string PickScene(NodeType type, int mapSeed, int nodeId)
    {
        IReadOnlyList<string> candidates = config.GetCandidates(type);
        if (candidates == null || candidates.Count == 0)
            return null;

        int combined = Hash(mapSeed, nodeId, (int)type);
        int index = PositiveMod(combined, candidates.Count);

        return candidates[index];
    }

    private int Hash(int a, int b, int c)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + a;
            h = h * 31 + b;
            h = h * 31 + c;
            return h;
        }
    }
    
    private int PositiveMod(int value, int mod)
    {
        int r = value % mod;
        return r < 0 ? r + mod : r;
    }
}