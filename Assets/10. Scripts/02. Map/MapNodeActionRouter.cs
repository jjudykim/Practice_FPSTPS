using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapNodeActionRouter : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private NodeScenePoolConfig poolConfig;

    [Header("Runtime")] [SerializeField] private int currentMapSeed;

    private NodeSceneSelector selector;

    private void Awake()
    {
        selector = new NodeSceneSelector(poolConfig);
    }

    public void OnNodeClicked(MapNode node)
    {
        if (node == null)
            return;

        string sceneName = selector.PickScene(node.Type, currentMapSeed, node.Id);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[MapNodeActionRouter] ::: No Scene candidates for type = {node.Type}");
            return;
        }
        
        Debug.Log($"[MapNodeActionRouter] ::: Load Scene : type = {node.Type}, nodeId={node.Id}, scene={sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void SetMapSeed(int seed)
    {
        currentMapSeed = seed;
    }
}