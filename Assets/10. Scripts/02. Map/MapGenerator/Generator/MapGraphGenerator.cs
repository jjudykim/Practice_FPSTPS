using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

// [Rules]
// - MainPath 1줄 필수 보장
// - 추가 간선은 총 예산 (edge budget)으로만 추가
// - 스킵 간선(depth +2 ~ +3)은 전체 추가 간선 중 일정 비율만 허용
// - 추가 간선은 후보에 점수를 매기고 상위 후보 위주로 선택
public class MapGraphGenerator : IMapGenerator
{
    private readonly int PRIME = 9973;

    private const int DefaultExtraEdgeBudgetPerDepth = 2;
    private const float DefaultSkipEdgeRatio = 0.15f;
    private const int DefaultMaxSkipDepth = 2;

    private const int DefaultLaneRadius = 1;
    private const int DefaultMaxIncoming = 2;

    public MapBuildDebugInfo LastDebugInfo { get; private set; }

    private class EdgeCandidate
    {
        public MapNode From { get; }
        public MapNode To { get; }
        public int FromDepth => From.Depth;
        public int ToDepth => To.Depth;

        public float Score { get; set; }
        public int LaneDistance { get; set; }
        public int IncomingBefore { get; set; }

        public EdgeCandidate(MapNode from, MapNode to)
        {
            From = from;
            To = to;
        }
    }
    
    // =======================================
    //             1. Generation
    // =======================================
    public MapContext Generate(MapBuildRequest request)
    {
        ValidateRequestOrThrow(request);

        int baseSeed = request.Seed;

        for (int attempt = 0; attempt <= request.MaxRetries; attempt++)
        {
            int attemptSeed = unchecked(request.Seed + attempt * PRIME);
            MapGraph graph = InternalBuild(request, baseSeed, attemptSeed);

            if (ValidateGraph(graph, request.TotalDepth))
                return new MapContext(request.MapId, baseSeed, attemptSeed, graph);
        }

        throw new InvalidOperationException("Failed to generate valid map.");
    }

    public static MapGraph GenerateFromData(MapData data)
    {
        if (data == null)
            return null;

        MapBuildRequest req = data.Request.ToRequest();
        MapGraphGenerator instance = new MapGraphGenerator();

        return instance.InternalBuild(req, req.Seed, data.UsedSeed);
    }

    private MapGraph InternalBuild(MapBuildRequest req, int baseSeed, int usedSeed)
    {
        Random rng = new Random(usedSeed);
        LastDebugInfo = new MapBuildDebugInfo(req.MapId, baseSeed, usedSeed, req.TotalDepth);

        // 1) Depth 별 노드를 먼저 전부 생성
        var nodes = CreateNodes(req, rng);
        var graph = new MapGraph(nodes);

        // 디버그용
        RecordDepthNodesForDebug(graph, req.TotalDepth);

        // 2) MainPath 보장 (Start -> Boss 1줄)
        CreateMainPath(graph, req.TotalDepth, rng, LastDebugInfo);
        
        // 3) 추가 간선 : 총 예산만큼 추가 + 스킵 간선은 비율 제한
        AddExtraEdges(graph, req, rng, LastDebugInfo);
        
        // 4) 노드 타입 배정
        new NodeTypeAssigner().AssignTypes(graph, req, rng);

        return graph;
    }
    
    private List<MapNode> CreateNodes(MapBuildRequest req, Random rng)
    {
        List<MapNode> nodes = new List<MapNode>();
        int nextNodeId = 0;
        for (int depth = 0; depth < req.TotalDepth; depth++)
        {
            int count = (depth == 0 || depth == req.TotalDepth - 1) 
                ? 1 : rng.Next(req.MinNodesPerDepth, req.MaxNodesPerDepth + 1);
            for (int i = 0; i < count; ++i)
            {
                NodeType type = (depth == 0) ? NodeType.Start 
                    : (depth == req.TotalDepth - 1) ? NodeType.Boss 
                    : NodeType.Combat;
                
                nodes.Add(new MapNode(nextNodeId++, depth, type));
            }
        }
        return nodes;
    }

    private void CreateMainPath(MapGraph graph, int totalDepth, Random rng, MapBuildDebugInfo debug)
    {
        MapNode current = graph.GetNodesAtDepth(0)[0];

        for (int depth = 1; depth < totalDepth; depth++)
        {
            List<MapNode> candidates = graph.GetNodesAtDepth(depth);
            MapNode next = candidates[rng.Next(candidates.Count)];

            if (AddEdge(current, next))
            {
                debug.AddMainPathEdge(current.Id, next.Id, depth);
            }

            current = next;
        }
    }

    private void AddExtraEdges(MapGraph graph, MapBuildRequest req, Random rng, MapBuildDebugInfo debug)
    {
        int totalDepth = req.TotalDepth;
        int extraBudget = Mathf.Max(0, totalDepth - 1) * DefaultExtraEdgeBudgetPerDepth;
        
        int skipBudget = Mathf.RoundToInt(extraBudget * DefaultSkipEdgeRatio);
        int normalBudget = extraBudget - skipBudget;

        // 간선 후보를 1) depth+1 후보 2) 스킵 후보 로 분리해서 각각 스코어링해 비교
        List<EdgeCandidate> normalCandidates = BuildCandidates(graph, req, 1);
        List<EdgeCandidate> skipCandidates = BuildCandidates(graph, req, DefaultMaxSkipDepth)
                                             .Where(c => c.ToDepth - c.FromDepth >= 2)
                                             .ToList();

        // 스코어 계산
        ScoreCandidates(graph, normalCandidates, DefaultLaneRadius, DefaultMaxIncoming);
        ScoreCandidates(graph, skipCandidates, DefaultLaneRadius, DefaultMaxIncoming);
        
        // 점수 높은 후보부터 상위 풀에서 랜덤 선택
        normalCandidates = normalCandidates.OrderByDescending(c => c.Score).ToList();
        skipCandidates = skipCandidates.OrderByDescending(c => c.Score).ToList();
        
        PickEdgesFromCandidates(graph, normalCandidates, normalBudget, rng, debug, MapBuildDebugInfo.EdgeBuildStage.ExtraEdge_Normal);
        PickEdgesFromCandidates(graph, skipCandidates, skipBudget, rng, debug, MapBuildDebugInfo.EdgeBuildStage.ExtraEdge_Skip);

        debug.SetExtraBudget(extraBudget, normalBudget, skipBudget);
    }

    private List<EdgeCandidate> BuildCandidates(MapGraph graph, MapBuildRequest req, int maxSkipDepth)
    {
        List<EdgeCandidate> list = new List<EdgeCandidate>();

        for (int fromDepth = 0; fromDepth < req.TotalDepth - 1; ++fromDepth)
        {
            List<MapNode> fromNodes = graph.GetNodesAtDepth(fromDepth);
            if (fromNodes.Count == 0)
                continue;
            
            int toDepthMax = Mathf.Min(req.TotalDepth - 1, fromDepth + maxSkipDepth);

            for (int toDepth = fromDepth + 1; toDepth <= toDepthMax; ++toDepth)
            {
                List<MapNode> toNodes = graph.GetNodesAtDepth(toDepth);
                if (toNodes.Count == 0)
                    continue;

                foreach (MapNode from in fromNodes)
                {
                    foreach (MapNode to in toNodes)
                    {
                        if (from.NextNodeIds.Contains(to.Id))
                            continue;
                        
                        list.Add(new EdgeCandidate(from, to));
                    }
                }
            }
        }

        return list;
    }

    private void ScoreCandidates(MapGraph graph, List<EdgeCandidate> candidates, int laneRadius, int maxIncoming)
    {
        Dictionary<int, Dictionary<int, int>> depthIndexCache = new Dictionary<int, Dictionary<int, int>>();

        int GetIndexInDepth(MapNode node)
        {
            if (depthIndexCache.TryGetValue(node.Depth, out var map) == false)
            {
                var nodes = graph.GetNodesAtDepth(node.Depth);
                map = new Dictionary<int, int>(nodes.Count);
                for (int i = 0; i < nodes.Count; ++i)
                    map[nodes[i].Id] = i;

                depthIndexCache[node.Depth] = map;
            }

            return map.TryGetValue(node.Id, out int idx) ? idx : 0;
        }

        foreach (var c in candidates)
        {
            int fromIdx = GetIndexInDepth(c.From);
            int toIdx = GetIndexInDepth(c.To);

            int depthDelta = c.ToDepth - c.FromDepth;
            int incoming = graph.GetIncomingCount(c.To.Id);
            
            // 스코어 설계
            float score = 0f;
            
            // 1) depthDelta가 1에 가까울수록 +, 멀수록 -
            score += (depthDelta == 1) ? 10f : (depthDelta == 2 ? 4f : 1f);
            
            // 2) 레인 근접이면 +, 멀면 -
            int laneDist = Mathf.Abs(toIdx - fromIdx);
            score += Mathf.Max(0f, 6f - laneDist * 2f);
            
            // 3) incoming 과다 억제 (합류 억제)
            score += (incoming < maxIncoming) ? 3f : -8f;
            
            // 4) Boss로의 과도한 점프 억제
            if (c.To.Type == NodeType.Boss && depthDelta >= 2)
                score -= 10f;

            c.Score = score;
            c.LaneDistance = laneDist;
            c.IncomingBefore = incoming;
        }
    }

    private void PickEdgesFromCandidates(MapGraph graph, List<EdgeCandidate> candidates, int budget, Random rng, MapBuildDebugInfo debug, MapBuildDebugInfo.EdgeBuildStage stage)
    {
        if (budget <= 0 || candidates.Count == 0)
            return;

        const int TopK = 12;

        int added = 0;
        int guard = 0;

        while (added < budget && candidates.Count > 0 && guard++ < 5000)
        {
            int poolCount = Mathf.Min(TopK, candidates.Count);
            int pickIndex = rng.Next(poolCount);

            EdgeCandidate pick = candidates[pickIndex];

            if (pick.From.Depth >= pick.To.Depth)
            {
                candidates.RemoveAt(pickIndex);
                continue;
            }

            if (pick.From.NextNodeIds.Contains(pick.To.Id))
            {
                candidates.RemoveAt(pickIndex);
                continue;
            }
            
            // 간선 추가
            if (AddEdge(pick.From, pick.To))
            {
                added++;
                debug.AddExtraEdge(pick.From.Id, pick.To.Id
                                 , pick.FromDepth, pick.ToDepth
                                 , pick.Score, pick.LaneDistance, pick.IncomingBefore, stage);
                
                candidates.RemoveAt(pickIndex);
            }
        }
    }

    private bool AddEdge(MapNode from, MapNode to)
    {
        if (to == null || from.NextNodeIds.Contains(to.Id))
            return false;
        
        from.NextNodeIds.Add(to.Id);
        return true;
    }
    
    // Debug용
    private void RecordDepthNodesForDebug(MapGraph graph, int totalDepth)
    {
        for (int d = 0; d < totalDepth; ++d)
        {
            List<MapNode> nodes = graph.GetNodesAtDepth(d);
            LastDebugInfo.AddDepthNodeList(d, nodes.Select(n => n.Id).ToList());
        }
    }
    
    
    // =======================================
    //             2. Validation
    // =======================================
    private bool ValidateGraph(MapGraph graph, int totalDepth)
    {
        // 1) Start -> Boss 도달 가능한지 검증
        if (IsReachable(graph, graph.StartNode.Id, graph.BossNode.Id) == false)
            return false;
        
        // 2) 너무 많은 depth가 도달 가능한 노드 1개만 가지는 경우는 노잼
        int nojamDepthCount = 0;
        for (int depth = 1; depth < totalDepth - 1; ++depth)
        {
            int reachableCount = graph.GetNodesAtDepth(depth).Count(n => IsReachable(graph, graph.StartNode.Id, n.Id));
            if (reachableCount <= 1)
                nojamDepthCount++;
        }

        int allowedNojam = (totalDepth - 2) / 2 + 1;
        return nojamDepthCount < allowedNojam;
    }

    private bool IsReachable(MapGraph graph, int startId, int targetId)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startId);
        visited.Add(startId);

        while (queue.Count > 0)
        {
            int curId = queue.Dequeue();
            if (curId == targetId)
                return true;

            foreach (int nextId in graph.GetNodeById(curId).NextNodeIds)
            {
                if (visited.Add(nextId))
                    queue.Enqueue(nextId);
            }
        }

        return false;
    }
    
    // =======================================
    //          3. Request Validation
    // =======================================
    private void ValidateRequestOrThrow(MapBuildRequest req)
    {
        if (req.TotalDepth < 2)
            throw new ArgumentException("TotalDepth must be >= 2 (Start + Boss).");
        if (req.MinNodesPerDepth < 1 || req.MaxNodesPerDepth < 1)
            throw new ArgumentException("NodesPerDepth must be >= 1.");
        if (req.MinNodesPerDepth > req.MaxNodesPerDepth)
            throw new ArgumentException("MinNodesPerDepth cannot be greater than MaxNodesPerDepth.");
        if (req.MinOutDegree < 1 || req.MaxOutDegree < 1)
            throw new ArgumentException("OutDegree must be >= 1.");
        if (req.MinOutDegree > req.MaxOutDegree)
            throw new ArgumentException("MinOutDegree cannot be greater than MaxOutDegree.");
        if (req.CrossChance < 0f || req.CrossChance > 1f)
            throw new ArgumentException("CrossChance must be in [0..1].");
        if (req.MaxRetries < 0)
            throw new ArgumentException("MaxRetries must be >= 0.");
    }
}



#region 기존 코드 백업용
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using Random = System.Random;
//
// public class MapGraphGenerator : IMapGenerator
// {
//     private readonly int PRIME = 9973;
//
//     private const int DefaultLaneRadius = 1;
//     private const int DefaultMaxIncoming = 2;
//
//     private const bool UseStepwiseRelaxation = true;
//     
//     // =======================================
//     //             1. Generation
//     // =======================================
//     public MapContext Generate(MapBuildRequest request)
//     {
//         ValidateRequestOrThrow(request);
//
//         for (int attempt = 0; attempt <= request.MaxRetries; attempt++)
//         {
//             int attemptSeed = unchecked(request.Seed + attempt * PRIME);
//             MapGraph graph = InternalBuild(request, attemptSeed);
//
//             if (ValidateGraph(graph, request.TotalDepth))
//             {
// #if UNITY_EDITOR
//                 DebugLogEdgeStats(graph, request.TotalDepth);
// #endif
//                 return new MapContext(request.MapId, request.Seed, graph);
//             }
//         }
//
//         throw new InvalidOperationException("Failed to generate valid map.");
//     }
//     
//     private MapGraph InternalBuild(MapBuildRequest req, int seed)
//     {
//         Random rng = new Random(seed);
//         var nodes = CreateNodes(req, rng);
//         var graph = new MapGraph(nodes);
//
//         CreateMainPath(graph, req.TotalDepth, rng);
//         AddExtraConnections(graph, req, rng);
//         
//         new NodeTypeAssigner().AssignTypes(graph, req, rng);
//         return graph;
//     }
//
//     private List<MapNode> CreateNodes(MapBuildRequest req, Random rng)
//     {
//         List<MapNode> nodes = new List<MapNode>();
//         int nextNodeId = 0;
//
//         for (int depth = 0; depth < req.TotalDepth; depth++)
//         {
//             int count = (depth == 0 || depth == req.TotalDepth - 1) 
//                 ? 1 : rng.Next(req.MinNodesPerDepth, req.MaxNodesPerDepth + 1);
//
//             for (int i = 0; i < count; ++i)
//             {
//                 NodeType type = (depth == 0) ? NodeType.Start 
//                               : (depth == req.TotalDepth - 1) ? NodeType.Boss 
//                               : NodeType.Combat;
//                 
//                 nodes.Add(new MapNode(nextNodeId++, depth, type));
//             }
//         }
//
//         return nodes;
//     }
//
//     private void CreateMainPath(MapGraph graph, int totalDepth, Random rng)
//     {
//         MapNode current = graph.GetNodesAtDepth(0)[0];
//         for (int depth = 1; depth < totalDepth; depth++)
//         {
//             List<MapNode> candidates = graph.GetNodesAtDepth(depth);
//             MapNode next = candidates[rng.Next(candidates.Count)];
//             
//             AddEdge(current, next);
//             current = next;
//         }
//     }
//     
//     private void AddExtraConnections(MapGraph graph, MapBuildRequest req, Random rng)
//     {
//         for (int depth = 0; depth < req.TotalDepth - 1; depth++)
//         {
//             List<MapNode> currentNodes = graph.GetNodesAtDepth(depth);
//             List<MapNode> nextNodes = graph.GetNodesAtDepth(depth + 1);
//
//             Dictionary<int, int> curIndex = BuildIndexMap(currentNodes);
//             Dictionary<int, int> nextIndex = BuildIndexMap(nextNodes);
//
//             Dictionary<int, int> incomingCounts = BuildIncomingCountMap(graph, nextNodes);
//
//             foreach (var node in currentNodes)
//             {
//                 int myIndex = curIndex[node.Id];
//                 int targetOutDegree = rng.Next(req.MinOutDegree, req.MaxOutDegree + 1);
//
//                 while (node.NextNodeIds.Count < targetOutDegree)
//                 {
//                     MapNode chosen = ChooseNextNode(node, myIndex
//                                                   , nextNodes, nextIndex
//                                                   , incomingCounts, rng);
//                     if (chosen == null)
//                         break;
//
//                     if (AddEdge(node, chosen))
//                         incomingCounts[chosen.Id]++;
//                     else
//                         break;
//                 }
//             }
//         }
//     }
//     
//     // =======================================
//     //          1-1. Selection Policy
//     // =======================================
//
//     private MapNode ChooseNextNode(MapNode current, int myIndex, List<MapNode> nextNodes,
//         Dictionary<int, int> nextIndex, Dictionary<int, int> incomingCounts, Random rng)
//     {
//         List<MapNode> candidates = new List<MapNode>();
//
//         for (int i = 0; i < nextNodes.Count; ++i)
//         {
//             MapNode n = nextNodes[i];
//             if (current.NextNodeIds.Contains(n.Id))
//                 continue;
//             
//             candidates.Add(n);
//         }
//
//         if (candidates.Count == 0)
//             return null;
//
//         List<MapNode> strict = FilterByLaneAndIncoming(candidates, myIndex
//                                                      , nextIndex, incomingCounts
//                                                      , DefaultLaneRadius, DefaultMaxIncoming);
//
//         List<MapNode> finalCandidates = strict;
//
//         if (finalCandidates.Count == 0)
//         {
//             if (UseStepwiseRelaxation)
//             {
//                 finalCandidates = FilterByLaneOnly(candidates, myIndex, nextIndex, DefaultLaneRadius);
//
//                 if (finalCandidates.Count == 0)
//                     finalCandidates = candidates;
//             }
//             else
//             {
//                 finalCandidates = candidates;
//             }
//         }
//
//         SortByLaneDistance(finalCandidates, myIndex, nextIndex);
//
//         int topN = Mathf.Min(3, finalCandidates.Count);
//         return finalCandidates[rng.Next(topN)];
//     }
//     
//     private static Dictionary<int, int> BuildIndexMap(List<MapNode> nodes)
//     {
//         Dictionary<int, int> map = new Dictionary<int, int>(nodes.Count);
//         for (int i = 0; i < nodes.Count; ++i)
//             map[nodes[i].Id] = i;
//
//         return map;
//     }
//
//     private static Dictionary<int, int> BuildIncomingCountMap(MapGraph graph, List<MapNode> nextNodes)
//     {
//         Dictionary<int, int> incoming = new Dictionary<int, int>(nextNodes.Count);
//         for (int i = 0; i < nextNodes.Count; i++)
//         {
//             MapNode n = nextNodes[i];
//             incoming[n.Id] = graph.GetIncomingCount(n.Id);
//         }
//         return incoming;
//     }
//     
//     private static List<MapNode> FilterByLaneAndIncoming(List<MapNode> nodes, int myIndex
//                                                         , Dictionary<int, int> nextIndex
//                                                         , Dictionary<int, int> incomingCounts
//                                                         , int laneRadius, int maxIncoming)
//     {
//         List<MapNode> result = new List<MapNode>();
//         for (int i = 0; i < nodes.Count; i++)
//         {
//             MapNode n = nodes[i];
//             int ni = nextIndex[n.Id];
//
//             if (Mathf.Abs(ni - myIndex) > laneRadius)
//                 continue;
//
//             if (incomingCounts[n.Id] >= maxIncoming)
//                 continue;
//
//             result.Add(n);
//         }
//         return result;
//     }
//     
//     private List<MapNode> FilterByLaneOnly(List<MapNode> candidates, int myIndex, Dictionary<int, int> nextIndex, int defaultLaneRadius)
//     {
//         throw new NotImplementedException();
//     }
//     
//     private void SortByLaneDistance(List<MapNode> finalCandidates, int myIndex, Dictionary<int, int> nextIndex)
//     {
//         throw new NotImplementedException();
//     }
//
//     private MapNode ChooseBestNextNode(MapNode current, int myIndex
//                                      , List<(MapNode node, int index)> nextNodes
//                                      , Dictionary<int, int> incomingCounts
//                                      , MapBuildRequest req, Random rng)
//     {
//         var candidates = nextNodes
//                                          .Where(x => current.NextNodeIds.Contains(x.node.Id) == false)
//                                          .ToList();
//
//         if (candidates.Any() == false)
//             return null;
//
//         var filtered = candidates
//                                        .Where(x => Mathf.Abs(x.index - myIndex) <= DefaultLaneRadius && incomingCounts[x.node.Id] < DefaultMaxIncoming)
//                                        .ToList();
//
//         var finalCandidates = filtered.Any() ? filtered : candidates;
//
//         return finalCandidates
//                .OrderBy(x => Mathf.Abs(x.index - myIndex))
//                .Take(3)
//                .ElementAt(rng.Next(Mathf.Min(3, finalCandidates.Count))).node;
//     }
//
//     private bool AddEdge(MapNode from, MapNode to)
//     {
//         if (to == null || from.NextNodeIds.Contains(to.Id))
//             return false;
//         
//         from.NextNodeIds.Add(to.Id);
//         return true;
//     }
//     
//     // =======================================
//     //             2. Validation
//     // =======================================
//     private bool ValidateGraph(MapGraph graph, int totalDepth)
//     {
//         // a) Start -> Boss 도달 가능한지 검증
//         if (IsReachable(graph, graph.StartNode.Id, graph.BossNode.Id) == false)
//             return false;
//         
//         // b) 너무 많은 depth가 "도달 가능한 노드 1개"만 가지는 경우 = 노잼
//         int nojamDepthCount = 0;
//         for (int depth = 1; depth < totalDepth - 1; depth++)
//         {
//             int reachableCount = graph.GetNodesAtDepth(depth).Count(n => IsReachable(graph, graph.StartNode.Id, n.Id));
//             
//             if (reachableCount <= 1)
//                 nojamDepthCount++;
//         }
//
//         int allowedNojam = (totalDepth - 2) / 2 + 1;
//         return nojamDepthCount < allowedNojam;
//     }
//
//     private bool IsReachable(MapGraph graph, int startId, int targetId)
//     {
//         HashSet<int> visited = new HashSet<int>();
//         Queue<int> queue = new Queue<int>();
//         queue.Enqueue(startId);
//         visited.Add(startId);
//
//         while (queue.Count > 0)
//         {
//             int curId = queue.Dequeue();
//             if (curId == targetId)
//                 return true;
//
//             foreach (int nextId in graph.GetNodeById(curId).NextNodeIds)
//             {
//                 if (visited.Add(nextId))
//                     queue.Enqueue(nextId);
//             }
//         }
//
//         return false;
//     }
//     
//     
//     // =======================================
//     //          3. Request Validation
//     // =======================================
//     private void ValidateRequestOrThrow(MapBuildRequest req)
//     {
//         if (req.TotalDepth < 2)
//             throw new ArgumentException("TotalDepth must be >= 2 (Start + Boss).");
//
//         if (req.MinNodesPerDepth < 1 || req.MaxNodesPerDepth < 1)
//             throw new ArgumentException("NodesPerDepth must be >= 1.");
//
//         if (req.MinNodesPerDepth > req.MaxNodesPerDepth)
//             throw new ArgumentException("MinNodesPerDepth cannot be greater than MaxNodesPerDepth.");
//
//         if (req.MinOutDegree < 1 || req.MaxOutDegree < 1)
//             throw new ArgumentException("OutDegree must be >= 1.");
//
//         if (req.MinOutDegree > req.MaxOutDegree)
//             throw new ArgumentException("MinOutDegree cannot be greater than MaxOutDegree.");
//
//         if (req.CrossChance < 0f || req.CrossChance > 1f)
//             throw new ArgumentException("CrossChance must be in [0..1].");
//
//         if (req.MaxRetries < 0)
//             throw new ArgumentException("MaxRetries must be >= 0.");
//     }
//     
//     
//     // =======================================
//     //             cf. RNG Helpers
//     // =======================================
//     private static int NextIntInclusive(System.Random rng, int min, int max) => rng.Next(min, max + 1); 
// }
#endregion