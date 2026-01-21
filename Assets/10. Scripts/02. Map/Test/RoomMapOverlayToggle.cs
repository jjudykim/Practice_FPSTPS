using UnityEngine;
using UnityEngine.UIElements;

public class RoomMapOverlayToggle : MonoBehaviour
{
    [Header("Input")] 
    [SerializeField] private KeyCode toggleKey = KeyCode.M;

    [Header("References")] 
    [SerializeField] private MapOverlayPanel overlayPanel;   

    [Header("Options")] 
    [SerializeField] private bool viewOnly = true;
    [SerializeField] private bool refreshOnOpen = true;

    private bool isOpen;

    private void Awake()
    {
        if (overlayPanel == null)
            overlayPanel = FindFirstObjectByType<MapOverlayPanel>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        if (overlayPanel != null)
            overlayPanel.gameObject.SetActive(false);

        isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    private void Toggle()
    {
        if (overlayPanel == null)
        {
            Debug.LogError("[RoomMapOverlayToggle] ::: overlayPanel is null. 씬에 MapOverlayPanel이 필요합니다.");
            return;
        }
        
        isOpen = !isOpen;
        overlayPanel.gameObject.SetActive(isOpen);

        if (isOpen && refreshOnOpen)
        {
            TryRefresh();
        }
    }

    private void TryRefresh()
    {
        if (GameManager.Instance == null)
            return;
        
        MapRunCache cache = GameManager.Instance.MapCache;
        
        if (cache == null || cache.HasGraph == false || cache.CurrentGraph == null)
            return;
        
        overlayPanel.Open(cache.CurrentGraph, viewOnly);
        overlayPanel.ApplyProgress(cache, viewOnly);
    }
}
