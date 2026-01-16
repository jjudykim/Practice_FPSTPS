using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MapSystem mapSystem;
    
    [Header("Build Request (Default)")]
    [SerializeField] private int mapId = 1;
    
    [Tooltip("true : 아래 seed 그대로 사용 / false : 실행시마다 랜덤 seed")]
    [SerializeField] private bool useFixedSeedForDebug = true;
    
    [SerializeField] private int seed = 12345;

    [Header("Debug HotKeys")] 
    [SerializeField] private KeyCode rebuildKey = KeyCode.R;
    [SerializeField] private KeyCode fixedKey = KeyCode.F;
    

    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindObjectOfType<MapSystem>();

        mapSystem.OnMapBuilt += HandleMapBuilt;
    }
    
    private void OnDestroy()
    {
        if (mapSystem != null)
            mapSystem.OnMapBuilt -= HandleMapBuilt;
    }

    private void Update()
    {
        if (Input.GetKeyDown(rebuildKey))
        {
            StartNewSession();
        }

        if (Input.GetKeyDown(fixedKey))
        {
            useFixedSeedForDebug = !useFixedSeedForDebug;
        }
    }

    public void StartNewSession()
    { 
        int newSeed = useFixedSeedForDebug ? seed : Random.Range(0, int.MaxValue);
        mapSystem.Build(mapId, newSeed);
    }

    public void BuildFromSharedSeed(int sharedSeed)
    {
        mapSystem.Build(mapId, sharedSeed);
    }

    private void HandleMapBuilt(MapContext ctx)
    {
        PrintGraph(ctx.Graph);
    }

    private void PrintGraph(MapGraph graph)
    {
        Debug.Log("=== MAP GRAPH (Diagram-aligned) ===");

        foreach (var node in graph.Nodes)
        {
            string next = node.NextNodeIds.Count == 0
                ? "-"
                : string.Join(", ", node.NextNodeIds);

            Debug.Log($"Node {node.Id} | Depth {node.Depth} | {node.Type} -> [{next}]");
        }
    }
}