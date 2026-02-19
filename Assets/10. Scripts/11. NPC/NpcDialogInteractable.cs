using TMPro;
using UnityEngine;

public class NpcDialogInteractable : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private string startNodeId = "START";
    [SerializeField] private string promptText = "대화";
    [SerializeField] private GameObject customDialogPrefab;

    [Header("World Space UI")] 
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private float interactDistance = 3f;

    private Transform playerTransform;

    public string StartNodeId => startNodeId;
    public string PromptText => promptText;
    public GameObject CustomDialogPrefab => customDialogPrefab;
    
    private void Start()
    {
        // Player 태그를 가진 오브젝트를 찾아 트랜스폼 캐싱
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (promptUI != null)
            promptUI.Hide();
    }
    
    private void Update()
    { 
        if (playerTransform == null || promptUI == null) 
            return;
        
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool isNear = dist <= interactDistance;
        
        if (isNear)
        {
            promptUI.Show($"[E] {promptText}");
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (DialogManager.Instance != null && DialogManager.Instance.IsOpen == false)
                {
                    DialogManager.Instance.Open(this, startNodeId);
                    promptUI.Hide();
                }
            }
        }
        else
        {
            promptUI.Hide();
        }
    }
}