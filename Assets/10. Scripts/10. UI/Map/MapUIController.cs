using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIController :  MonoBehaviour
{
    public enum MapUIMode
    {
        ViewOnly,
        Interactive
    }

    [Header("Dependencies")] 
    [SerializeField] private MapSystem mapSystem;
    
    [Header("Roots")] 
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform graphRoot;
    [SerializeField] private RectTransform edgesRoot;
    [SerializeField] private RectTransform nodesRoot;
    
    [Header("Prefabs")]
    [SerializeField] private MapEdgeView edgePrefab;
    [SerializeField] private MapNodeView nodePrefab;
    
    [Header("Scroll")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private float contentPaddingY = 200f;
    [SerializeField] private float contentPaddingX = 0f;
    
    [Header("Layout")]
    [SerializeField] private float primarySpacing = 220f;
    [SerializeField] private float secondarySpacing = 120f;

    [Header("Movement Rule")] 
    [SerializeField] private bool allowBacktracking = true;

    [Header("Node Action Router")] 
    [SerializeField] private MapNodeActionRouter actionRouter;
    
    private readonly List<GameObject> spawned = new();

    private readonly Dictionary<int, MapNode> nodesById = new();
    private readonly Dictionary<int, MapNodeView> nodeViewsById = new();
    private readonly Dictionary<int, HashSet<int>> neighborsById = new();
    private readonly List<MapEdgeView> edgeViews = new();
    
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private MapUIMode currentMode = MapUIMode.Interactive;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (actionRouter == null)
            actionRouter = FindFirstObjectByType<MapNodeActionRouter>();
    }

    public void Toggle(MapGraph graph, MapUIMode mode)
    {
        if (IsOpen)
            Close();
        else
        {
            if (mode == MapUIMode.Interactive)
                Open(graph, false);
            else
                Open(graph, true);
        }
    }

    public void Open(MapGraph graph)
    {
        Open(graph, false);
    }

    public void Open(MapGraph graph, bool viewOnly)
    {
        if (graph == null || panelRoot == null)
            return;
        
        currentMode = viewOnly ? MapUIMode.ViewOnly : MapUIMode.Interactive;
        panelRoot.SetActive(true);       
 
        ClearUI();
        ClearCaches();
        
        // 1. 노드 데이터 캐싱
        for (int i = 0; i < graph.Nodes.Count; ++i)
            nodesById[graph.Nodes[i].Id] = graph.Nodes[i];

        // 2. 레이아웃 계산
        var settings = new MapGraphLayout.LayoutSettings(primarySpacing
                                                       , secondarySpacing
                                                       , new Vector2(contentPaddingX, contentPaddingY)
                                                       , MapGraphLayout.FlowDirection.BottomToTop);
        
        Dictionary<int, Vector2> positions = MapGraphLayout.BuildNodePositions(graph, settings);

        // 3. 그래프 경계 계산
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        foreach (var p in positions.Values)
        {
            minX = Mathf.Min(minX, p.x); 
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }
        
        // 4. 컨텐츠 크기 결정
        float graphWidth = maxX - minX;
        float graphHeight = maxY - minY;
        float finalWidth = Mathf.Max(graphWidth + contentPaddingX * 2f, scrollRect.viewport.rect.width);
        float finalHeight = graphHeight + contentPaddingY * 2f;

        content.sizeDelta = new Vector2(finalWidth, finalHeight);
        
        // 5. 오프셋 계산
        float offsetX = -(minX + maxX) / 2f;
        float offsetY = (-finalHeight / 2f + contentPaddingY) - minY;
        Vector2 totalOffset = new Vector2(offsetX, offsetY);
        BuildNeighbors(graph);
        
        // 노드 생성 및 배치
        foreach (var node in graph.Nodes)
        {
            if (positions.TryGetValue(node.Id, out Vector2 pos) == false)
                continue;

            var view = Instantiate(nodePrefab, nodesRoot);
            spawned.Add(view.gameObject);
            nodeViewsById[node.Id] = view;

            RectTransform rt = view.GetComponent<RectTransform>();
            rt.anchoredPosition = pos + totalOffset;

            view.Bind(node, viewOnly ? null : (Action<int>)HandleNodeClicked);
            view.SetRaycastBlock(!viewOnly);
        }
        
        // 간선 생성 및 배치
        foreach (var node in graph.Nodes)
        {
            if (node.NextNodeIds == null || node.NextNodeIds.Count == 0)
                continue;

            if (positions.TryGetValue(node.Id, out Vector2 startPos) == false)
                continue;

            foreach (int nextId in node.NextNodeIds)
            {
                if (positions.TryGetValue(nextId, out Vector2 endPos) == false)
                    continue;
                
                var edge = Instantiate(edgePrefab, edgesRoot);
                spawned.Add(edge.gameObject);
                
                edge.Set(node.Id, nextId, startPos + totalOffset, endPos + totalOffset);
                edgeViews.Add(edge);
            }
        }
        
        if (content != null)
        {
            // 앵커를 중앙(0.5, 0.5)으로 설정
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
        }
       
        if (graphRoot != null)
        {
            // 맵이 그려지는 루트도 중앙으로 고정
            graphRoot.anchorMin = new Vector2(0.5f, 0.5f);
            graphRoot.anchorMax = new Vector2(0.5f, 0.5f);
            graphRoot.pivot = new Vector2(0.5f, 0.5f);
            graphRoot.anchoredPosition = Vector2.zero; // 위치 리셋
        }
        
        Canvas.ForceUpdateCanvases();
        
        if (nodesRoot != null)
            nodesRoot.SetAsLastSibling();
        
        if (edgesRoot != null)
            edgesRoot.SetAsFirstSibling();
    }

    private void BuildNeighbors(MapGraph graph)
    {
        neighborsById.Clear();
        
        foreach (var n in graph.Nodes)
            neighborsById[n.Id] = new HashSet<int>();

        foreach (var from in graph.Nodes)
        {
            if (from.NextNodeIds == null) continue;
            
            foreach (int toId in from.NextNodeIds)
            {
                neighborsById[from.Id].Add(toId);
                if (allowBacktracking && neighborsById.ContainsKey(toId))
                    neighborsById[toId].Add(from.Id);
            }
        }
    }

    public void SnapToNode(int nodeId)
    {
        if (scrollRect == null || content == null)
            return;

        if (nodeViewsById.TryGetValue(nodeId, out MapNodeView targetView) == false)
            return;
        
        Canvas.ForceUpdateCanvases();

        RectTransform targetRT = targetView.GetComponent<RectTransform>();
        RectTransform viewportRT = scrollRect.viewport;
        
        Vector3 nodeWorldPos = targetRT.position;
        Vector3[] viewportCorners = new Vector3[4];
        viewportRT.GetWorldCorners(viewportCorners);
        Vector3 viewportCenterWorld = (viewportCorners[0] + viewportCorners[2]) * 0.5f;
        
        Vector3 worldDelta = viewportCenterWorld - nodeWorldPos;
        
        Vector3 localDelta = content.parent.InverseTransformVector(worldDelta);
        
        Vector2 newPos = content.anchoredPosition;
        newPos.y += localDelta.y;
        content.anchoredPosition = newPos;
        
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        
        Debug.Log($"[MapUI] SnapToNode({nodeId}) ::: NodeWorldY={nodeWorldPos.y}, ViewCenterWorldY={viewportCenterWorld.y}, DeltaY={localDelta.y}, FinalAnchoredY={content.anchoredPosition.y}");
    }

    private int FindStartNodeId(MapGraph graph)
    {
        if (graph == null || graph.Nodes == null)
            return -1;

        for (int i = 0; i < graph.Nodes.Count; ++i)
        {
            if (graph.Nodes[i].Type == NodeType.Start)
                return graph.Nodes[i].Id;
        }

        return (graph.Nodes.Count > 0) ? graph.Nodes[0].Id : -1;
    }

    private void ResizeContentToFit(Dictionary<int, Vector2> positions)
    {
        if (content == null || positions == null || positions.Count == 0)
            return;

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        
        foreach(var pair in positions)
        {
            Vector2 p = pair.Value;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }
        
        float width = (maxX - minX) + contentPaddingX * 2f;
        float height = (maxY - minY) + contentPaddingY * 2f;
        
        RectTransform viewport = scrollRect.viewport;
        width  = Mathf.Max(width, viewport.rect.width + 1f);
        height = Mathf.Max(height, viewport.rect.height + 1f);


        content.sizeDelta = new Vector2(width, height);
    }

    private void Close()
    {
        if (panelRoot == null)
            return;
        
        panelRoot.SetActive(false);
        
        ClearUI();
        ClearCaches();
    }

    private void ClearUI()
    {
        for (int i = 0; i < spawned.Count; ++i)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }

        spawned.Clear();
    }

    private void ClearCaches()
    {
        nodesById.Clear();
        nodeViewsById.Clear();
        edgeViews.Clear();
        neighborsById.Clear();
    }

    private void HandleNodeClicked(int id)
    {
        Debug.Log($"[MapUI] Node Clicked: {id}");
        
        if (currentMode == MapUIMode.ViewOnly)
             return;
        
        if (actionRouter == null)
                actionRouter = FindFirstObjectByType<MapNodeActionRouter>();
        
        var cache = Managers.Instance.Game.MapCache;
        if (cache == null || cache.CurrentGraph == null)
            return;

        if (nodesById.TryGetValue(id, out var node) == false || node == null)
            return;
        
        if (cache.CurrentNodeId < 0)
        {
            int startId = FindStartNodeId(cache.CurrentGraph);
            if (id != startId)
                return;
        }
        else
        {
            bool isReclickingCurrentStart = (id == cache.CurrentNodeId && node.Type == NodeType.Start);

            if (isReclickingCurrentStart)
            {
                // Start 노드에서는 본인 씬 진입 허용
            }
            else
            {
                if (neighborsById.TryGetValue(cache.CurrentNodeId, out var nb) == false || nb.Contains(id) == false)
                    return;    
            }
        }
        
        actionRouter.OnNodeClicked(node);
    }

    public void ApplyProgress(MapRunCache cache, MapUIMode mode)
    {
        if (cache == null || cache.CurrentGraph == null)
            return;

        if (cache.CurrentNodeId < 0)
        {
            int startId = FindStartNodeId(cache.CurrentGraph);
            Debug.Log("startNodeId : " + startId);

            foreach (var kv in nodeViewsById)
            {
                int nodeId = kv.Key;
                MapNodeView view = kv.Value;
                
                if (nodeId == startId)
                    view.ApplyState(MapNodeView.NodeUIState.Available);
                else
                    view.ApplyState(MapNodeView.NodeUIState.Locked);
                
                if (mode == MapUIMode.ViewOnly)
                    view.SetRaycastBlock(false);
            }

            for (int i = 0; i < edgeViews.Count; ++i)
            {
                if (edgeViews[i] != null)
                    edgeViews[i].SetState(MapEdgeView.EdgeUIState.Default);
            }

            return;
        }

        nodesById.TryGetValue(cache.CurrentNodeId, out MapNode currentNode);

        HashSet<int> nextSet = new HashSet<int>();
        if (neighborsById.TryGetValue(cache.CurrentNodeId, out var nb))
        {
            foreach (int nodeId in nb)
                nextSet.Add(nodeId);
        }

        foreach (var pair in nodeViewsById)
        {
            int nodeId = pair.Key;
            MapNodeView view = pair.Value;

            MapNodeView.NodeUIState state;

            if (nodeId == cache.CurrentNodeId)
                state = MapNodeView.NodeUIState.Current;
            else if (nextSet.Contains(nodeId))
                state = MapNodeView.NodeUIState.Available;
            else if (cache.ClearedNodeIds.Contains(nodeId))
                state = MapNodeView.NodeUIState.Cleared;
            else
                state = MapNodeView.NodeUIState.Locked;

            view.ApplyState(state);
            
            if (mode == MapUIMode.ViewOnly)
                view.SetRaycastBlock(false);
        }

        for (int i = 0; i < edgeViews.Count; ++i)
        {
            MapEdgeView edge = edgeViews[i];
            if (edge == null)
                continue;

            bool isCandidate = currentNode != null &&
                               (
                                   (edge.FromId == cache.CurrentNodeId && nextSet.Contains(edge.ToId)) ||
                                   (edge.ToId == cache.CurrentNodeId && nextSet.Contains(edge.FromId))
                               );

            if (isCandidate)
            {
                edge.SetState(MapEdgeView.EdgeUIState.Candidate);
                continue;
            }

            bool isVisited = cache.IsVisitedEdge(edge.FromId, edge.ToId) 
                           || cache.IsVisitedEdge(edge.ToId, edge.FromId);

            if (isVisited)
            {
                edge.SetState(MapEdgeView.EdgeUIState.Visited);
                continue;
            }

            edge.SetState(MapEdgeView.EdgeUIState.Default);
        }
    }
}