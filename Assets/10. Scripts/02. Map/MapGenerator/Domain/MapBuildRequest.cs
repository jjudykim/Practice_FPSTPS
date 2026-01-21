public class MapBuildRequest
{
    // Infos ============================================
    public int MapId { get; private set; }
    public int Seed { get; private set; }

    // Create Room Rules =================================
    public int TotalDepth { get; private set; }
    public int MinNodesPerDepth { get; private set; }
    public int MaxNodesPerDepth { get; private set; }
    public int MinOutDegree { get; private set; }
    public int MaxOutDegree { get; private set; }
    public float CrossChance { get; private set; }
    public int MaxRetries { get; private set; }
    
    // Room Type Rules ===================================
    public float RewardRatio { get; private set; }
    public float ShopRatio { get; private set; }
    public int RewardCoolDownDepth { get; private set; }
    public int ShopCoolDownDepth { get; private set; }
    public int RewardMinDepth { get; private set; }
    public int RewardMaxDepth { get; private set; }
    public int ShopMinDepth { get; private set; }
    public int ShopMaxDepth { get; private set; }

    private MapBuildRequest() { }
    

    public class Builder
    {
        private readonly MapBuildRequest req = new MapBuildRequest();

        public Builder(int mapId, int seed)
        {
            req.MapId = mapId;
            req.Seed = seed;
            
            // 필수적인 맵 생성 로직
        }

        public Builder SetGraphSize(int totalDepth, int minNodes, int maxNodes)
        {
            req.TotalDepth = totalDepth;
            req.MinNodesPerDepth = minNodes;
            req.MaxNodesPerDepth = maxNodes;
            return this;
        }

        public Builder SetConnectionRules(int minOut, int maxOut, float crossChance, int maxRetries)
        {
            req.MinOutDegree = minOut;
            req.MaxOutDegree = maxOut;
            req.CrossChance = crossChance;
            req.MaxRetries = maxRetries;
            return this;
        }

        public Builder SetRewardRules(float ratio, int cooldown, int minDepth, int maxDepth)
        {
            req.RewardRatio = ratio;
            req.RewardCoolDownDepth = cooldown;
            req.RewardMinDepth = minDepth;
            req.RewardMaxDepth = maxDepth;
            return this;
        }

        public Builder SetShopRules(float ratio, int cooldown, int minDepth, int maxDepth)
        {
            req.ShopRatio = ratio;
            req.ShopCoolDownDepth = cooldown;
            req.ShopMinDepth = minDepth;
            req.ShopMaxDepth = maxDepth;
            return this;
        }

        public MapBuildRequest Build() => req;
    }
}