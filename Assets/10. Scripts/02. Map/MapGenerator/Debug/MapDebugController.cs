using UnityEngine;

public class MapDebugController : MonoBehaviour
{
    [Header("References (Map Scene Only")]
    [SerializeField] private MapSystem mapSystem;
    [SerializeField] private MapSaveTestRunner mapSaveTestRunner;
    [SerializeField] private MapUIController mapUI;
    
    [Header("Debug HotKeys")]
    [SerializeField] private KeyCode rebuildKey = KeyCode.R;
    [SerializeField] private KeyCode fixedKey = KeyCode.F;
    
    [Header("Session HotKey (Delegated To Runner")]
    [SerializeField] private KeyCode newRandomSeedSessionKey = KeyCode.T;

    [Header("Build Request (Default)")] 
    [SerializeField] private int mapId = 1;
    
    [Tooltip("true : 아래 seed 그대로 사용 / false : 실행시마다 랜덤 seed")]
    [SerializeField] private bool useFixedSeedForDebug = true;
    [SerializeField] private int seed = 12345;

    [Header("View Only On/Off")] 
    [SerializeField] private MapUIController.MapUIMode mode;

    [Header("Auto Start")] 
    [SerializeField] private bool autoStartOnSceneLoad = true;
    [SerializeField] private bool openUIAfterBuild = true;

    private GameManager game;
    
    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();

        if (mapSaveTestRunner == null)
            mapSaveTestRunner = FindFirstObjectByType<MapSaveTestRunner>();

        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>();

        if (mapSystem != null)
            mapSystem.OnMapBuilt += HandleMapBuilt;
        
        game = Managers.Instance.Game;
    }

    private void Start()
    {
        if (game != null && game.HasActiveMap)
        {
            var cache = game.MapCache;

            if (mapUI != null && cache.CurrentGraph != null)
            {
                mapUI.Open(cache.CurrentGraph, false);
                mapUI.ApplyProgress(game.MapCache, MapUIController.MapUIMode.Interactive);
            }

            return;
        }

        if (autoStartOnSceneLoad)
            StartNewSession();
    }

    private void OnDestroy()
    {
        if (mapSystem != null)
            mapSystem.OnMapBuilt -= HandleMapBuilt;
    }

    private void Update()
    {
        if (Input.GetKeyDown(rebuildKey))
            StartNewSession();
        
        if (Input.GetKeyDown(fixedKey))
            useFixedSeedForDebug = !useFixedSeedForDebug;
        
        if (Input.GetKeyDown(newRandomSeedSessionKey))
        {
            if (mapSaveTestRunner != null)
                mapSaveTestRunner.BuildRandomFromSeedCache();
            
            if (mapUI != null && mapSystem != null && mapSystem.CurrentMap != null)
                mapUI.Toggle(mapSystem.CurrentMap.Graph, mode);
        }
    }
    
    public void StartNewSession()
    {
        if (mapSystem == null)
        {
            Debug.LogError("[MapDebugController] StartNewSession failed: mapSystem is null");
            return;
        }

        int newSeed = useFixedSeedForDebug ? seed : Random.Range(0, int.MaxValue);
        mapSystem.Build(mapId, newSeed);
    }
    
    private void HandleMapBuilt(MapContext ctx)
    {
        if (ctx == null || ctx.Graph == null)
        {
            Debug.LogError("[MapDebugController] HandleMapBuilt failed: ctx/graph is null");
            return;
        }
        
        game.SetCurrentMap(ctx.Graph, ctx.UsedSeed);

        if (mapUI != null && openUIAfterBuild)
        {
            bool viewOnly = (mode == MapUIController.MapUIMode.ViewOnly);
            mapUI.Open(ctx.Graph, viewOnly);
            
            mapUI.ApplyProgress(game.MapCache, mode);
        }
    }
}