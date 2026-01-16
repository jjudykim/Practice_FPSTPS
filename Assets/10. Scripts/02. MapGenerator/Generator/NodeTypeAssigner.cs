using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class NodeTypeAssigner
{
    private const float SPECIAL_RATIO = 0.45f;
    
    public void AssignTypes(MapGraph graph, MapBuildRequest req, Random rng)
    {
        List<MapNode> middleNodes = graph.Nodes
                                    .Where(n => n.Type != NodeType.Start && n.Type != NodeType.Boss)
                                    .ToList();
        
        // 목표 개수 산정(비율 기반으로)
        int targetRewardCount = Mathf.RoundToInt(middleNodes.Count * req.RewardRatio);
        int targetShopCount = Mathf.RoundToInt(middleNodes.Count * req.ShopRatio);

        int rewardCooldown = 0;
        int shopCooldown = 0;
        
        // 연속 방지 관리를 위해 depth별로 처리
        // - 같은 depth 안에서는 여러개 배치 가능
        // - 연속 -> depth 간의 연속으로 보고 depth 기준으로 쿨다운 적용
        for (int depth = 1; depth <= req.TotalDepth - 2; depth++)
        {
            rewardCooldown = Math.Max(0, rewardCooldown - 1);
            shopCooldown = Math.Max(0, shopCooldown - 1);
            
            List<MapNode> nodesAtDepth = middleNodes.Where(n => n.Depth == depth).ToList();
            if (nodesAtDepth.Count == 0)
                continue;

            if (rng.NextDouble() > SPECIAL_RATIO)
                continue;

            bool canReward = targetRewardCount > 0 
                             && rewardCooldown == 0 
                             && depth >= req.RewardMinDepth 
                             && depth <= req.RewardMaxDepth;

            bool canShop = targetShopCount > 0 
                           && shopCooldown == 0 
                           && depth >= req.ShopMinDepth 
                           && depth <= req.ShopMaxDepth;

            if (canReward == false && canShop == false)
                continue;

            NodeType chosen = canReward && canShop 
                              ? (rng.NextDouble() < 0.5 ? NodeType.Reward : NodeType.Shop)
                              : (canReward ? NodeType.Reward : NodeType.Shop);

            MapNode pick = nodesAtDepth[rng.Next(nodesAtDepth.Count)];
            pick.Type = chosen;

            if (chosen == NodeType.Reward)
            {
                targetRewardCount--;
                rewardCooldown = req.RewardCoolDownDepth;
            }
            else
            {
                targetShopCount--;
                shopCooldown = req.ShopCoolDownDepth;
            }
        }
    }
}