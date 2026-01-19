using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public int SaveVersion = 1;
    public string SavedAtUtc;
    
    public int MapId;
    public int UsedSeed;

    public MapBuildRequestData Request;

    public static MapData Create(int mapId, int usedSeed, MapBuildRequestData req)
    {
        return new MapData
        {
            SaveVersion = 1,
            SavedAtUtc = DateTime.Now.ToString("o"),
            MapId = mapId,
            UsedSeed = usedSeed,
            Request = req
        };
    }
    
    public float RewardRatio;
    public int RewardCooldownDepth;
    public float ShopRatio;
    public int ShopCooldownDepth;

    public int GeneratorVersion;
}

[Serializable]
public class MapBuildRequestData
{
    public int MapId;
    public int Seed;
    
    public int TotalDepth;
    public int MinNodesPerDepth;
    public int MaxNodesPerDepth;
    public int MinOutDegree;
    public int MaxOutDegree;
    public float CrossChance;
    public int MaxRetries;

    public float RewardRatio;
    public float ShopRatio;
    public int RewardCoolDownDepth;
    public int ShopCoolDownDepth;
    public int RewardMinDepth;
    public int RewardMaxDepth;
    public int ShopMinDepth;
    public int ShopMaxDepth;

    public static MapBuildRequestData FromMapSystemSettings(MapSystem mapSystem, int mapId, int seed)
    {
        return new MapBuildRequestData
        {
            MapId = mapId,
            Seed = seed,
            
            TotalDepth = mapSystem.TotalDepth,
            MinNodesPerDepth = mapSystem.MinNodesPerDepth,
            MaxNodesPerDepth = mapSystem.MaxNodesPerDepth,
            
            MinOutDegree = mapSystem.minOutDegree,
            MaxOutDegree = mapSystem.maxOutDegree,
            CrossChance = mapSystem.crossChance,
            MaxRetries = mapSystem.maxRetries,
            
            RewardRatio = mapSystem.rewardRatio,
            RewardCoolDownDepth = mapSystem.rewardCooldownDepth,
            ShopRatio = mapSystem.shopRatio,
            ShopCoolDownDepth = mapSystem.shopCooldownDepth,
            
            RewardMinDepth = 2,
            RewardMaxDepth = mapSystem.TotalDepth - 2,
            ShopMinDepth = 2,
            ShopMaxDepth = mapSystem.TotalDepth - 2,
        };
    }

    public MapBuildRequest ToRequest()
    {
        return new MapBuildRequest.Builder(MapId, Seed)
            .SetGraphSize(TotalDepth, MinNodesPerDepth, MaxNodesPerDepth)
            .SetConnectionRules(MinOutDegree, MaxOutDegree, CrossChance, MaxRetries)
            .SetRewardRules(RewardRatio, RewardCoolDownDepth, RewardMinDepth, RewardMaxDepth)
            .SetShopRules(ShopRatio, ShopCoolDownDepth, ShopMinDepth, ShopMaxDepth)
            .Build();
    }
}