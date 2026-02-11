using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapNodeActionRouter : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private NodeScenePoolConfig poolConfig;

    [Header("Runtime")] 
    [SerializeField] private int currentMapSeed;

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
        Debug.Log("[MapNodeActionRouter] ::: Pick한 씬 이름 : " + sceneName);
        if (string.IsNullOrEmpty(sceneName))
            return;
        
        Managers.Instance.Game.SelectNextNodeAndMove(node.Id);
        Managers.Instance.Scene.LoadScene(sceneName);
    }

    public void SetMapSeed(int seed)
    {
        currentMapSeed = seed;
    }
}