using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapSystem : MonoBehaviour
{
    [Header("Graph Settings")] 
    [SerializeField] private int totalDepth = 5;
    [SerializeField] private int minNodesPerDepth = 2;
    [SerializeField] private int maxNodesPerDepth = 3;

    public int TotalDepth => totalDepth;
    public int MinNodesPerDepth => minNodesPerDepth;
    public int MaxNodesPerDepth => maxNodesPerDepth;
    
    [Header("Connection Settings")] 
    [SerializeField] public int minOutDegree = 1;
    [SerializeField] public int maxOutDegree = 2;
    public int MinOutDegree => minOutDegree;
    public int MaxOutDegree => maxOutDegree;
    
    [Range(0f, 1f)] 
    [SerializeField] public float crossChance = 0.25f;
    [SerializeField] public int maxRetries = 20;
    public float CrossChance => crossChance;
    public int MaxRetries => maxRetries;
    
    [Header("Special Room Settings")]
    [Range(0f, 1f)]
    [SerializeField] public float rewardRatio = 0.1f;
    [SerializeField] public int rewardCooldownDepth = 2;
    public float RewardRatio => rewardRatio;
    public int RewardCooldownDepth => rewardCooldownDepth;
    
    [Range(0f, 1f)] 
    [SerializeField] public float shopRatio = 0.1f;
    [SerializeField] public int shopCooldownDepth = 2;
    public float ShopRatio => shopRatio;
    public int ShopCooldownDepth => shopCooldownDepth;
    
    [Header("MapList Build (Optional)")]
    [SerializeField] private MapListManager mapListManager;
    
    [Header("Debug Build Visualization")]
    [SerializeField] private bool debugBuild = true;
    [SerializeField] private float stepDelaySeconds = 0.35f;
    public bool DebugBuild => debugBuild;
    public float StepDelaySeconds => stepDelaySeconds;
    
    [Serializable]
    public class MapBuildDebugView
    {
        public MapBuildDebugInfo DebugInfo;

        // 현재 공개된 최대 depth (0..N)
        public int RevealedMaxDepth = -1;

        // 현재까지 공개된 간선들(메인 + extra)
        public List<MapBuildDebugInfo.EdgeBuildRecord> RevealedEdges = new List<MapBuildDebugInfo.EdgeBuildRecord>();

        public string CurrentStageLabel = "Idle";

        public void RevealAll(MapContext ctx)
        {
            if (ctx == null || DebugInfo == null)
                return;
            RevealedMaxDepth = DebugInfo.TotalDepth - 1;

            RevealedEdges.Clear();
            RevealedEdges.AddRange(DebugInfo.MainPathEdges);
            RevealedEdges.AddRange(DebugInfo.ExtraEdges);

            CurrentStageLabel = "All Revealed";
        }
    }
    
    public MapContext CurrentMap { get; private set; }
    public event Action<MapContext> OnMapBuilt;

    public MapBuildDebugView DebugView { get; private set; } = new MapBuildDebugView();

    private IMapGenerator generator;

    private void Awake()
    {
        generator = new MapGraphGenerator();
    }

    /// <summary>
    /// GameManager가 호출하는 진입점 : Build(MapID, Seed)
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="seed"></param>
    public void Build(int mapId, int seed)
    {
        StopAllCoroutines();
        ClearCurrentMap();
        ResetDebugView();

        MapBuildRequest request = BuildRequest(mapId, seed);
        CurrentMap = generator.Generate(request);

        if (generator is MapGraphGenerator mg)
            DebugView.DebugInfo = mg.LastDebugInfo;

        DebugView.RevealAll(CurrentMap);
        
        OnMapBuilt?.Invoke(CurrentMap);
    }
    
    private MapBuildRequest BuildRequest(int mapId, int seed)
    {
        return new MapBuildRequest.Builder(mapId, seed)
            .SetGraphSize(TotalDepth, minNodesPerDepth, maxNodesPerDepth)
            .SetConnectionRules(minOutDegree, maxOutDegree, crossChance, maxRetries)
            .SetRewardRules(rewardRatio, rewardCooldownDepth, 2, TotalDepth - 2)
            .SetShopRules(shopRatio, shopCooldownDepth, 2, TotalDepth - 2)
            .Build();
    }
    

    private void ResetDebugView()
    {
        DebugView = new MapBuildDebugView();
    }

    private void ClearCurrentMap()
    {
        CurrentMap = null;
        // 현재 노드 진행 상태 초기화
        // 맵 UI 캐시 제거
        // 룸 프리팹 / 오브젝트 참조 정리 ...
        // 등을 수행하는 곳으로
    }
}