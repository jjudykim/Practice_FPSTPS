using System;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [Header("Modern UI Pack")] [SerializeField]
    private ButtonManager buttonManager;

    [Header("Raycast Targets (assign if needed)")] [SerializeField]
    private Graphic[] raycastTargets;

    public int NodeId { get; private set; }
    public int Depth { get; private set; }

    private Action<int> onClicked;
    private MapNode boundNode;

    private Image normalImg;
    private Image highlightImg;
    private Image disabledImg;

    private Color baseColor = Color.gray;

    [SerializeField] private float currentNormalBoost = 1.25f;

    public enum NodeUIState
    {
        Normal,
        Current,
        Available,
        Cleared,
        Locked
    }

    private struct NodeStyle
    {
        public string label;
        public Color color;

        public NodeStyle(string label, Color color)
        {
            this.label = label;
            this.color = color;
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

        baseColor = style.color;
        buttonManager.SetText(style.label);

        ApplyState(NodeUIState.Normal);
    }

    private NodeStyle GetNodeStyle(MapNode node)
    {
        switch (node.Type)
        {
            case NodeType.Start: return new NodeStyle("START", Color.cyan);
            case NodeType.Boss: return new NodeStyle("BOSS", Color.red);
            case NodeType.Reward: return new NodeStyle("REWARD", Color.yellow);
            case NodeType.Shop: return new NodeStyle("SHOP", Color.green);
            default: return new NodeStyle(node.Type.ToString().ToUpper(), Color.gray);
        }
    }

    private Color GetStateColor(NodeUIState state)
    {
        switch (state)
        {
            case NodeUIState.Current:
                return Color.Lerp(baseColor, Color.darkBlue, 0.55f);
            case NodeUIState.Available:
                return Color.Lerp(baseColor, Color.white, 0.25f);
            case NodeUIState.Cleared:
                return Color.Lerp(baseColor, Color.gray, 0.55f);
            case NodeUIState.Locked:
                return Color.Lerp(baseColor, Color.black, 0.55f);
            case NodeUIState.Normal:
            default:
                return baseColor;
        }
    }


    public void ApplyState(NodeUIState state)
    {
        if (buttonManager == null || normalImg == null || highlightImg == null || disabledImg == null)
            return;

        float normalAlpha = 0.7f;
        float highlightAlpha = 1.0f;
        float disabledAlpha = 0.3f;

        bool interactable = true;

        switch (state)
        {
            case NodeUIState.Current:
                interactable = false;
                normalAlpha = 0.85f;
                highlightAlpha = 1.00f;
                disabledAlpha = 0.35f;
                break;
            case NodeUIState.Available:
                interactable = true;
                normalAlpha = 0.80f;
                highlightAlpha = 1.00f;
                disabledAlpha = 0.35f;
                break;
            case NodeUIState.Cleared:
                interactable = true;
                normalAlpha = 0.65f;
                highlightAlpha = 0.85f;
                disabledAlpha = 0.25f;
                break;
            case NodeUIState.Locked:
                interactable = false;
                normalAlpha = 0.25f;
                highlightAlpha = 0.25f;
                disabledAlpha = 0.20f;
                break;
            case NodeUIState.Normal:
            default:
                interactable = true;
                break;
        }

        Color stateColor = GetStateColor(state);
        
        normalImg.color = new Color(stateColor.r, stateColor.g, stateColor.b, normalAlpha);
        highlightImg.color = new Color(stateColor.r, stateColor.g, stateColor.b, highlightAlpha);
        disabledImg.color = new Color(stateColor.r, stateColor.g, stateColor.b, disabledAlpha);
        
        buttonManager.Interactable(interactable);
        buttonManager.UpdateUI();
    }

    public void SetRaycastBlock(bool block)
    {
        if (raycastTargets == null || raycastTargets.Length == 0)
            raycastTargets = GetComponentsInChildren<Graphic>(true);
        
        for (int i = 0; i < raycastTargets.Length; ++i)
        {
            if (raycastTargets[i] == null)
                continue;
            
            raycastTargets[i].raycastTarget = block;
        }
    }
}