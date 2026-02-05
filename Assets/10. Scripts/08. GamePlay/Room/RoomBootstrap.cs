using UnityEngine;

public class RoomBootstrap : MonoBehaviour
{
    [SerializeField] private RoomControllerBase roomController;

    private void Awake()
    {
        if (roomController == null)
            roomController = FindFirstObjectByType<RoomControllerBase>();

        if (Managers.Instance == null || Managers.Instance.Game == null)
        {
            Debug.LogError("[RoomBootstrapper] Managers.Instance.Game is null.");
            return;
        }

        int currentNodeId = Managers.Instance.Game.MapCache.CurrentNodeId;
        roomController.Init(currentNodeId);

        roomController.OnRoomFinished += HandleRoomFinished;

        Debug.Log($"[RoomBootstrapper] Room initialized. nodeId={currentNodeId}");
    }

    private void OnDestroy()
    {
        if (roomController != null)
            roomController.OnRoomFinished -= HandleRoomFinished;
    }

    private void HandleRoomFinished(RoomResult result)
    {
        Debug.Log($"[RoomBootstrapper] Room finished. type={result.Type}, nodeId={result.NodeId}");

        // GameManager에 결과를 전달 → 다음 노드 선택 UI로 넘어가는건 GameManager가 책임
        Managers.Instance.Game.OnRoomFinished(result);
    }
}