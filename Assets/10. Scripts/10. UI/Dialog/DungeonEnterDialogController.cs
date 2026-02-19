using UnityEngine;

public class DungeonEnterDialogController : DialogController
{ 
    [Header("Dungeon Settings")] 
    [Tooltip("던전에 진입하기로 결정된 최종 노드 ID 또는 선택지 ID")]
    [SerializeField] private string enterSuccessId = "ENTER_CONFIRMED"; 
    [SerializeField] private string targetSceneName = "MapScene"; 
    
    private bool isEnteringDungeon = false;
    protected override void OnChoicePicked(string fromNodeId, string pickedNextId)
    {
        Debug.Log("[DungeonEnterDialogController] ::: pickNextId: " +  pickedNextId);
        if (pickedNextId == enterSuccessId)
        {
            Debug.Log("[DungeonEnterDialogController] ::: isEnteringDungeon true");
            isEnteringDungeon = true;
        }
    }
    protected override void OnDialogFinished()
    {
        if (isEnteringDungeon)
        {
            Debug.Log("[DungeonEnterDialogController] ::: Call Start DungeonRun");
            StartDungeonRun();
        }
    }
    private void StartDungeonRun()
    {
        Debug.Log("[DungeonEnterDialogController] ::: Starting Dungeon Run...");
        // 1. GameManager를 통한 런 초기화 및 맵 생성
        if (Managers.Instance.Game != null)
        {
            Managers.Instance.Game.StartGame();
        }
        
        // 2. 맵 씬으로 이동
        if (Managers.Instance.Scene != null)
        {
            Managers.Instance.Scene.LoadScene(targetSceneName);
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        isEnteringDungeon = false;
    }
}