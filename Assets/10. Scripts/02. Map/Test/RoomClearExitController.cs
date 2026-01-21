using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomClearExitController : MonoBehaviour
{ 
    [Header("UI")]
    [SerializeField] private Button clearButton;
    [SerializeField] private Button exitButton;

    [Header("Scene")] 
    [SerializeField] private string mapSceneName = "TestMapScene";

    private bool isCleared;

    private void Awake()
    {
        if (clearButton != null)
            clearButton.onClick.AddListener(onClickClear);

        if (exitButton != null)
            exitButton.onClick.AddListener(onClickExit);
    }

    private void Start()
    {
        if (exitButton != null)
            exitButton.gameObject.SetActive(false);

        isCleared = false;
    }

    private void OnDestroy()
    {
        if (clearButton != null)
            clearButton.onClick.RemoveListener(onClickClear);
        
        if (exitButton != null)
            clearButton.onClick.RemoveListener(onClickExit);
    }

    private void onClickClear()
    {
        if (isCleared)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("[RoomClearExitController] ::: GameManager.Instance is null");
            return;
        }

        MapRunCache cache = GameManager.Instance.MapCache;
        if (cache == null || cache.HasGraph == false)
        {
            Debug.LogError("[RoomClearExitController] ::: MapCache is empty (no graph)");
            return;
        }

        int currentNodeId = cache.CurrentNodeId;
        if (currentNodeId < 0)
        {
            Debug.LogWarning("[RoomClearExitController] ::: currentNodeId < 0");
            return;
        }

        cache.ClearedNodeIds.Add(currentNodeId);
        isCleared = true;
        Debug.Log($"[RoomClearExitController] ::: Cleared node : {currentNodeId}");
        
        if (exitButton != null)
            exitButton.gameObject.SetActive(true);
    }

    private void onClickExit()
    {
        if (isCleared == false)
            return;

        if (string.IsNullOrEmpty(mapSceneName))
            return;
        
        SceneManager.LoadScene(mapSceneName);
   }
}