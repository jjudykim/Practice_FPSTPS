using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MapSystem이 만든 Graph를 Scene View에서 시각화하는 디버그용 컴포넌트.
/// - GameObject 하나 만들어서 붙이고
/// - mapSystem만 연결해두면 됩니다.
/// 
/// [Layout 변경]
/// - Depth(진행)는 위 -> 아래로 내려가도록 Y축에 배치
/// - 같은 Depth 안에서는 좌 -> 우로 X축에 펼침
/// </summary>
public class MapGraphGizmosDebugger : MonoBehaviour
{
    [SerializeField] private MapSystem mapSystem;

    [Header("Layout (Top -> Bottom)")]
    [Tooltip("Depth(진행 단계) 간 Y 간격. 위에서 아래로 내려가므로 보통 양수로 두고 내부에서 -로 적용합니다.")]
    [SerializeField]
    private float depthSpacingY = 3.0f;

    [SerializeField] private float nodeSpacingX = 2.0f;

    [Header("Draw")] [SerializeField] private float nodeRadius = 0.25f;
    [SerializeField] private bool drawNodeLabels = true;
    [SerializeField] private bool drawEdgeScoreLabels = true;

    private readonly Dictionary<int, Vector3> cachedPositions = new Dictionary<int, Vector3>();

    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindObjectOfType<MapSystem>();

        if (mapSystem != null)
            mapSystem.OnMapBuilt += HandleMapBuilt;
    }

    private void OnDestroy()
    {
        if (mapSystem != null)
            mapSystem.OnMapBuilt -= HandleMapBuilt;
    }

    private void HandleMapBuilt(MapContext ctx)
    {
        CachePositions(ctx.Graph);
    }

    private void CachePositions(MapGraph graph)
    {
        cachedPositions.Clear();

        // depth별로 노드들을 가져와서 배치
        // depth = Y축(위->아래 진행) / 인덱스 = X축(좌->우 분산)
        for (int depth = 0; depth <= 9999; depth++)
        {
            List<MapNode> nodesAtDepth = graph.GetNodesAtDepth(depth);
            if (nodesAtDepth == null || nodesAtDepth.Count == 0)
                break;

            int count = nodesAtDepth.Count;
            float centerOffset = (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                MapNode node = nodesAtDepth[i];

                float x = (i - centerOffset) * nodeSpacingX;
                float y = -depth * depthSpacingY;

                cachedPositions[node.Id] = transform.position + new Vector3(x, y, 0f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mapSystem == null || mapSystem.CurrentMap == null)
            return;

        MapGraph graph = mapSystem.CurrentMap.Graph;
        if (graph == null)
            return;

        if (cachedPositions.Count == 0)
            CachePositions(graph);

        var view = mapSystem.DebugView;
        int revealDepth = (view != null) ? view.RevealedMaxDepth : int.MaxValue;

        
        // 1) Node 그리기
        DrawNodes(graph, revealDepth);
        
        // 2) Edge 먼저 그리기
        DrawEdges(view);



#if UNITY_EDITOR
        if (view != null && drawNodeLabels)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, $"[MapDebug] {view.CurrentStageLabel}");
        }
#endif
    }

    private void DrawEdges(MapSystem.MapBuildDebugView view)
    {
        if (view == null || view.RevealedEdges == null)
            return;

        foreach (var e in view.RevealedEdges)
        {
            if (!cachedPositions.TryGetValue(e.FromId, out Vector3 from))
                continue;
            if (!cachedPositions.TryGetValue(e.ToId, out Vector3 to))
                continue;
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(from, to);

#if UNITY_EDITOR
            if (drawEdgeScoreLabels)
            {
                // 메인 경로는 score=0이므로 라벨을 다르게
                string label = (e.Stage == MapBuildDebugInfo.EdgeBuildStage.MainPath)
                    ? "Main"
                    : $"{e.Stage} | S:{e.Score:0.0} | L:{e.LaneDistance} | In:{e.IncomingBefore}";

                Vector3 mid = (from + to) * 0.5f;
                UnityEditor.Handles.Label(mid + Vector3.up * 0.1f, label);
            }
#endif
        }
    }

    private void DrawNodes(MapGraph graph, int revealDepth)
    {
        foreach (MapNode node in graph.Nodes)
        {
            if (node.Depth > revealDepth)
                continue;

            if (!cachedPositions.TryGetValue(node.Id, out Vector3 pos))
                continue;

            Gizmos.color = GetColorByType(node.Type);
            Gizmos.DrawSphere(pos, nodeRadius);

#if UNITY_EDITOR
            if (drawNodeLabels)
            {
                UnityEditor.Handles.Label(pos + Vector3.up * 0.2f, $"{node.Id} (D{node.Depth}) [{node.Type}]");
            }
#endif
        }
    }

    private Color GetColorByType(NodeType type)
    {
        // 색은 취향이라 최소한만 구분
        switch (type)
        {
            case NodeType.Start: return Color.green;
            case NodeType.Boss: return Color.red;
            case NodeType.Reward: return Color.yellow;
            case NodeType.Shop: return Color.cyan;
            default: return Color.gray; // Combat
        }
    }
}