using UnityEngine;

public class ShopRoomController : RoomControllerBase
{
    [Header("Room Points")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private EndPointTrigger endPoint;

     public override void Init(int nodeId)
     {
        base.Init(nodeId);

        // 1) 플레이어 배치
        if (entryPoint != null)
        {
            Player.Instance.transform.SetPositionAndRotation(entryPoint.position, entryPoint.rotation);
        }

        // 2) 카메라 모드 전환
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetMode(CameraController.CameraMode.FirstPerson);
        }

        // 3) 탈출구 즉시 활성화
        if (endPoint != null)
        {
            endPoint.SetActive(true);
        }

        Debug.Log($"[ShopRoom] Init. NodeId: {nodeId}");
    }

    public override void OnPlayerReachedEndPoint()
    {
        if (isFinished) return;

        // 보상방 클리어 처리 (기본 보상 키 설정 가능)
        FinishCleared(rewardKey: "SHOP_DEFAULT");
    }
}