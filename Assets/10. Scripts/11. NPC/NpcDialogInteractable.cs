using UnityEngine;

public class NpcDialogInteractable : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private string startNodeId = "START";
    [SerializeField] private string promptText = "대화";
    
    [Header("On Dialog Finished")]
    [SerializeField] private bool loadSceneOnDialogFinish = false;
    [SerializeField] private string nextSceneName = "";

    public string StartNodeId => startNodeId;
    public string PromptText => promptText;
    
    public bool LoadSceneOnDialogFinish => loadSceneOnDialogFinish;
    public string NextSceneName => nextSceneName;
}