using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapSceneBootstrapper : MonoBehaviour
{
    [Header("References (Map Scene Only)")]
    [SerializeField] private MapSystem mapSystem;
    [SerializeField] private MapUIController mapUI;
    
    [SerializeField] private MapListManager mapListManager;

    [Header("Entry Options")]
    [SerializeField] private bool openUIOnEnter = true;
    [SerializeField] private bool applyProgressOnOpen = true;
    
    [Header("Cursor Reset Policy")]
    [SerializeField] private bool resetCursorOnEnter = true;

    [Tooltip("MapScene에서는 커서를 보이게 할지")]
    [SerializeField] private bool cursorVisible = true;

    [Tooltip("MapScene에서는 커서를 잠금 해제할지")]
    [SerializeField] private CursorLockMode lockMode = CursorLockMode.None;

    [Header("Optional Cursor Texture")]
    [SerializeField] private bool applyCursorTexture = false;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private MapPresetRepository repo;

    private void OnEnable()
    {
        if (resetCursorOnEnter)
            ResetCursorState();

        // UI 포커스 꼬임 방지(선택)
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void Awake()
    {
        if (mapSystem == null)
            mapSystem = FindFirstObjectByType<MapSystem>();

        if (mapUI == null)
            mapUI = FindFirstObjectByType<MapUIController>(FindObjectsInactive.Include);

        if (mapListManager == null)
            mapListManager = FindFirstObjectByType<MapListManager>(FindObjectsInactive.Include);
    }

    private IEnumerator Start()
    {
        yield return EnsureSceneReferences();

        yield return EnsureGameManagerReady();

        yield return EnsureMapListReadyIfNeeded();

        if (TryOpenFromExistingCache())
            yield break;

        bool built = TryBuildFromListOrPreset();
    }

    private IEnumerator EnsureSceneReferences()
    {
        int safetyFrames = 3;

        while (safetyFrames-- > 0)
        {
            if (mapSystem != null && mapUI != null)
                yield break;

            if (mapSystem == null)
                mapSystem = FindFirstObjectByType<MapSystem>();

            if (mapUI == null)
                mapUI = FindFirstObjectByType<MapUIController>(FindObjectsInactive.Include);

            yield return null;
        }
        
        if (mapSystem == null)
            Debug.LogError("[MapSceneBootstrapper] mapSystem is null. Place MapSystem in the scene.");

        if (mapUI == null)
            Debug.LogError("[MapSceneBootstrapper] mapUI is null. Place MapUIController in the scene.");
    }

    private IEnumerator EnsureGameManagerReady()
    {
        const float timeoutSeconds = 5f;
        float t = 0f;

        while (Managers.Instance.Game == null)
        {
            t += Time.unscaledDeltaTime;
            if (t >= timeoutSeconds)
            {
                Debug.LogError("[MapSceneBootstrapper] GameManager.Instance is still null (timeout).");
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator EnsureMapListReadyIfNeeded()
    {
        if (mapListManager == null)
            yield break;

        bool ok = false;
        yield return mapListManager.LoadMapList(done => ok = done);

        if (ok == false)
            Debug.LogError("[MapSceneBootstrapper] MapList load failed.");
    }

    private bool TryOpenFromExistingCache()
    {
        if (Managers.Instance.Game == null)
            return false;

        MapRunCache cache = Managers.Instance.Game.MapCache;

        if (cache == null || cache.HasGraph == false || cache.CurrentGraph == null)
            return false;

        if (openUIOnEnter == false)
            return true;

        if (mapUI == null)
            return true;

        mapUI.Open(cache.CurrentGraph, false);

        if (applyProgressOnOpen)
            mapUI.ApplyProgress(cache, MapUIController.MapUIMode.Interactive);
        
        int nodeToSnap = cache.CurrentNodeId;

        if (cache.CurrentNodeId < 0 && cache.CurrentGraph != null)
        {
            nodeToSnap = cache.CurrentGraph.StartNode.Id;
        }
       
        if (cache.CurrentNodeId >= 0)
        {
            mapUI.SnapToNode(cache.CurrentNodeId);
        }

        Debug.Log($"[MapSceneBootstrapper] ::: Opened UI from cache. seed={cache.CurrentSeed}, current={cache.CurrentNodeId}, cleared={cache.ClearedNodeIds.Count}");
        return true;
    }

    private bool TryBuildFromListOrPreset()
    {
        repo = new MapPresetRepository();
        repo.RebuildSeedCache();

        if (repo.TryPickRandomSeed(out int seed) == false)
            return false;

        if (repo.TryLoadPresetBySeed(seed, out MapData data) == false)
            return false;
        
        mapSystem.Build(data.MapId, data.UsedSeed);
        return true;
    }
    
    private void ResetCursorState()
    {
        Cursor.lockState = lockMode;
        Cursor.visible = cursorVisible;

        if (applyCursorTexture)
        {
            // cursorTexture가 null이면 기본 커서로
            Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
        }

        Debug.Log($"[MapSceneBootStrap] Cursor Reset -> lockState={Cursor.lockState}, visible={Cursor.visible}");
    }
}
