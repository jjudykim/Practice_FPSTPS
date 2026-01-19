using System;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [Header("Modern UI Pack")]
    [SerializeField] private ButtonManager buttonManager;
    
    public int NodeId { get; private set; }
    public int Depth { get; private set; }

    private Action<int> onClicked;
    private MapNode boundNode;

    private Image normalImg;
    private Image highlightImg;
    private Image disabledImg;

    private struct NodeStyle
    {
        public string label;
        public Color color;
        public bool interactable;

        public NodeStyle(string label, Color color, bool interactable)
        {
            this.label = label;
            this.color = color;
            this.interactable = interactable;
        }
    }

    private void Reset()
    {
        if (buttonManager == null)
            buttonManager = GetComponent<ButtonManager>();
    }

    public void Bind(MapNode node, Action<int> onClick)
    {
        boundNode = node;
        NodeId = node.Id;
        Depth = node.Depth;
        this.onClicked = onClick;

        ResolveVisualImages();
        
        ApplyStyleByType(node);
        HookClickEventOnce();
    }

    private void ResolveVisualImages()
    {
        if (buttonManager == null)
            return;

        normalImg = buttonManager.normalCG.GetComponentInChildren<Image>();
        highlightImg = buttonManager.highlightCG.GetComponentInChildren<Image>();
        disabledImg = buttonManager.disabledCG.GetComponentInChildren<Image>();
    }

    private void HookClickEventOnce()
    {
        if (buttonManager == null)
        {
            Debug.LogError("[MapNodeView] ::: ButtonManager is null");
            return;
        }
        
        buttonManager.onClick.RemoveListener(HandleClick);
        buttonManager.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        if (buttonManager != null)
            buttonManager.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        onClicked?.Invoke(NodeId);
    }
    
    private void ApplyStyleByType(MapNode node)
    {
        if (buttonManager == null)
            return;

        NodeStyle style = GetNodeStyle(node);

        Color c = style.color;
        normalImg.color = new Color(c.r, c.g, c.b, c.a * 0.7f);
        highlightImg.color = c;
        disabledImg.color = new Color(c.r, c.g, c.b, c.a * 0.3f);
        buttonManager.SetText(style.label);
        buttonManager.Interactable(style.interactable);
    }

    private NodeStyle GetNodeStyle(MapNode node)
    {
        switch (node.Type)
        {
            case NodeType.Start:
                return new NodeStyle("START", Color.cyan, true);
            case NodeType.Boss:
                return new NodeStyle("BOSS", Color.red, true);
            case NodeType.Reward:
                return new NodeStyle("REWARD", Color.yellow, true);
            case NodeType.Shop:
                return new NodeStyle("SHOP", Color.green, true);
            default:
                return new NodeStyle(node.Type.ToString().ToUpper(), Color.gray, true);
        }
    }
}