using UnityEngine;
using UnityEngine.UI;

public class MapEdgeView : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float thickness = 4f;
    [SerializeField] private Image lineImage;

    public int FromId { get; private set; } = -1;
    public int ToId { get; private set; } = -1;

    public enum EdgeUIState
    {
        Default,
        Visited,
        Candidate
    }

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

    public void Set(int fromId, int toId, Vector2 start, Vector2 end)
    {
        FromId = fromId;
        ToId = toId;
        
        Set(start, end);
    }

    public void SetState(EdgeUIState state)
    {
        if (lineImage == null)
            return;

        Color c = lineImage.color;

        switch (state)
        {
            case EdgeUIState.Candidate:
                c.a = 1.0f;
                break;
            case EdgeUIState.Visited:
                c.a = 0.2f;
                break;
            case EdgeUIState.Default:
            default:
                c.a = 0.5f;
                break;
        }

        lineImage.color = c;
    }

    public void SetEmphasis(bool on)
    {
        if (lineImage == null)
            return;

        Color c = lineImage.color;
        c.a = on ? 1.0f : 0.15f;

        lineImage.color = c;
    }

    public void SetThickness(float value)
    {
        thickness = Mathf.Max(1f, value);
    }
}