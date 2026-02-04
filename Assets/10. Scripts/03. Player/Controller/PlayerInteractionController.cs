using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerInteractionController : MonoBehaviour
{
    [Header("Interaction Key")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Settings")]
    [Tooltip("대화 중에는 상호작용 키를 막을지 여부")]
    [SerializeField] private bool blockWhileDialogOpen = true;
    
    private readonly List<NpcDialogInteractable> candidates = new List<NpcDialogInteractable>();
    private NpcDialogInteractable currentTarget;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.Hide();
        
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[PlayerInteractionController] Player interaction collider should be IsTrigger=true.");
        }
    }

    private void Update()
    {
        if (blockWhileDialogOpen && DialogManager.Instance != null && DialogManager.Instance.IsOpen)
            return;
        
        if (Input.GetKeyDown(interactKey))
        {
            if (currentTarget == null)
                return;
            
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.Open(currentTarget, currentTarget.StartNodeId);
                
                if (promptUI != null)
                    promptUI.Hide();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        NpcDialogInteractable npc = other.GetComponentInParent<NpcDialogInteractable>();
        if (npc == null)
            return;

        if (!candidates.Contains(npc))
            candidates.Add(npc);

        RefreshTargetAndUI();
    }

    private void OnTriggerExit(Collider other)
    {
        NpcDialogInteractable npc = other.GetComponentInParent<NpcDialogInteractable>();
        if (npc == null)
            return;

        candidates.Remove(npc);

        RefreshTargetAndUI();
    }

    private void RefreshTargetAndUI()
    {
        if (candidates.Count == 0)
        {
            currentTarget = null;
            if (promptUI != null)
                promptUI.Hide();
            return;
        }
        
        currentTarget = GetNearestCandidate();

        if (promptUI != null && currentTarget != null)
        {
            string label = string.IsNullOrWhiteSpace(currentTarget.PromptText) ? "대화하기" : currentTarget.PromptText;
            promptUI.Show($"{interactKey} - {label}");
        }
    }

    private NpcDialogInteractable GetNearestCandidate()
    {
        NpcDialogInteractable best = null;
        float bestSqr = float.MaxValue;

        Vector3 pos = transform.position;

        for (int i = 0; i < candidates.Count; i++)
        {
            NpcDialogInteractable c = candidates[i];
            if (c == null)
                continue;

            float sqr = (c.transform.position - pos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = c;
            }
        }
        
        candidates.RemoveAll(x => x == null);

        return best;
    }
}
