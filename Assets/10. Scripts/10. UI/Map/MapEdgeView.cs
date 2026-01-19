using UnityEngine;
using UnityEngine.UI;

public class MapEdgeView : MonoBehaviour
{
    [SerializeField] private float thickness = 4f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        
        Image img = GetComponent<Image>();
        img.raycastTarget = false;
    }

    public void Set(Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        float length = dir.magnitude;

        Vector2 mid = (start + end) * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rect.anchoredPosition = mid;
        rect.sizeDelta = new Vector2(length, thickness);
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetThickness(float value)
    {
        thickness = Mathf.Max(1f, value);
    }
}