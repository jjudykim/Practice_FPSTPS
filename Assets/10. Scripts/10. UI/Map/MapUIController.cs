using UnityEngine;

public class MapUIController :  MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private MapSystem mapSystem;
    
    [Header("Roots")] 
    [SerializeField] private RectTransform nodePrefab;
    [SerializeField] private RectTransform edgeRoot;
    
    // 

    public void SetUIDirection(Vector3 target)
    {
        
    }
}