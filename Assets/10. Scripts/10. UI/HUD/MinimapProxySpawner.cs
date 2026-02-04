using UnityEngine;

public class MinimapProxySpawner : MonoBehaviour
{
    [Header("Proxy Prefab")]
    [SerializeField] private GameObject proxyPrefab;

    [Header("Proxy Settings")]
    [SerializeField] private bool matchBounds = true;
    [SerializeField] private float yOffset = 0.05f;  
    [SerializeField] private Transform proxyParent;  

    [Header("Manual Size (when matchBounds = false)")]
    [SerializeField] private Vector3 manualSize = new Vector3(1f, 0.2f, 1f);
    [SerializeField] private Vector3 manualRot = new Vector3(0f, 0f, 0f);

    private GameObject spawnedProxy;

    private void Start()
    {
        if (proxyPrefab == null)
        {
            Debug.LogWarning($"[MinimapProxySpawner] proxyPrefab is null. name={name}");
            return;
        }

        SpawnProxy();
    }

    private void SpawnProxy()
    {
        spawnedProxy = Instantiate(proxyPrefab);
        
        if (proxyParent != null)
            spawnedProxy.transform.SetParent(proxyParent, true);
        
        spawnedProxy.transform.position = transform.position + Vector3.up * yOffset;
        spawnedProxy.transform.rotation = transform.rotation;
        
        if (matchBounds)
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null)
            {
                Bounds b = r.bounds;
                
                Vector3 size = b.size;
                size.y = Mathf.Max(0.1f, size.y * 0.2f);
                
                spawnedProxy.transform.position = b.center + Vector3.up * yOffset;
                
                spawnedProxy.transform.localScale = size;
            }
            else
            {
                spawnedProxy.transform.localScale = manualSize;
            }
        }
        else
        {
            spawnedProxy.transform.localScale = manualSize;
            spawnedProxy.transform.localEulerAngles = manualRot;
        }
        
        SetLayerRecursively(spawnedProxy, LayerMask.NameToLayer("MinimapGeometry"));
    }

    private void OnDestroy()
    {
        if (spawnedProxy != null)
            Destroy(spawnedProxy);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;

        go.layer = layer;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }
    }
}
