using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.UI;

public class MapUIController :  MonoBehaviour
{
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

    [Header("Node Action Router")] 
    [SerializeField] private MapNodeActionRouter actionRouter;
    
    private readonly List<GameObject> spawned = new();

    private MapGraph currentGraph;
    private readonly Dictionary<int, MapNode> nodeById = new Dictionary<int, MapNode>();
    
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Toggle(MapGraph graph)
    {
        if (IsOpen)
            Close();
        else
            Open(graph);
    }

    private void Open(MapGraph graph)
    {
        if (panelRoot == null)
        {
            Debug.LogError("[MapUIController] panelRoot is null");
            return;
        }
        
        panelRoot.SetActive(true);
        Clear();

        currentGraph = graph;

        nodeById.Clear();
        for (int i = 0; i < graph.Nodes.Count; ++i)
        {
            MapNode n = graph.Nodes[i];
            nodeById[n.Id] = n;
        }

        var settings = new MapGraphLayout.LayoutSettings(primarySpacing
                                                       , secondarySpacing
                                                       , new Vector2(contentPaddingX, contentPaddingY)
                                                       , MapGraphLayout.FlowDirection.BottomToTop);
        
        Dictionary<int, Vector2> positions = MapGraphLayout.BuildNodePositions(graph, settings);

        ResizeContentToFit(positions);

        foreach (var node in graph.Nodes)
        {
            if (positions.TryGetValue(node.Id, out Vector2 pos) == false)
                continue;

            var view = Instantiate(nodePrefab, nodesRoot);
            spawned.Add(view.gameObject);

            RectTransform rt = view.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            
            view.Bind(node, HandleNodeClicked);
        }

        foreach (var node in graph.Nodes)
        {
            if (node.NextNodeIds == null || node.NextNodeIds.Count == 0)
                continue;

            if (positions.TryGetValue(node.Id, out Vector2 start) == false)
                continue;

            foreach (int nextId in node.NextNodeIds)
            {
                if (positions.TryGetValue(nextId, out Vector2 end) == false)
                    continue;
                
                var edge = Instantiate(edgePrefab, edgesRoot);
                spawned.Add(edge.gameObject);
                
                edge.Set(start, end);
            }
        }

        SnapToStart();
    }

    private void SnapToStart()
    {
        if (scrollRect == null)
            return;
        
        Canvas.ForceUpdateCanvases();
        
        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;
        
        scrollRect.verticalNormalizedPosition = 0f;
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

        RectTransform viewport = scrollRect.viewport;
        float width = (maxX - minX) + contentPaddingX * 2f;
        float height = (maxY - minY) + contentPaddingY * 2f;
        
        width  = Mathf.Max(width, width + 1f);
        height = Mathf.Max(height, height + 1f);


        content.sizeDelta = new Vector2(width, height);
    }

    private void Close()
    {
        if (panelRoot == null)
            return;
        
        panelRoot.SetActive(false);
        Clear();
    }

    private void Clear()
    {
        for (int i = 0; i < spawned.Count; ++i)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }

        spawned.Clear();

        currentGraph = null;
        nodeById.Clear();
    }

    private void HandleNodeClicked(int id)
    {
        if (actionRouter == null)
        {
            Debug.LogError("[MapUIController] actionRouter is null. Assign it in Inspector.");
            return;
        }

        if (nodeById.TryGetValue(id, out MapNode node) == false || node == null)
        {
            Debug.LogError($"[MapUIController] ::: Cannot find MapNode by id = {id}");
            return;
        }

        actionRouter.OnNodeClicked(node);
    }
}