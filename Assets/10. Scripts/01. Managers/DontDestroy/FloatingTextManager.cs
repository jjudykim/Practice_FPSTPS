using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager
{
    private GameObject prefab;
    private Transform root;
    private Queue<FloatingText> pool = new Queue<FloatingText>();

    public void Init()
    {
        prefab = Resources.Load<GameObject>("Prefabs/UI/FloatingText");

        root = new GameObject("@FloatingText_Root").transform;
        root.SetParent(Managers.Instance.transform);
    }

    public void Show(Vector3 worldPos, string text, Color color)
    {
        FloatingText ft = GetFromPool();
        ft.transform.position = worldPos;
        ft.gameObject.SetActive(true);
        
        ft.Play(text, color);
    }

    private FloatingText GetFromPool()
    {
        if (pool.Count > 0)
        {
            var ft = pool.Dequeue();
            if (ft != null)
                return ft;
        }

        var go = Object.Instantiate(prefab, root);
        return go.GetComponent<FloatingText>();
    }

    public void ReturnToPool(FloatingText ft) => pool.Enqueue(ft);

}