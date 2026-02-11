using UnityEngine;

public class BossRoomController : RoomControllerBase
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform playerEntryPoint;
   
    private EnemyController spawnedBoss;
    private float startTime;

    public override void Init(int nodeId)
    {
        base.Init(nodeId);
        startTime = Time.time;

        // 1. 플레이어 배치
        if (playerEntryPoint != null && Player.Instance != null)
        {
            Player.Instance.transform.SetPositionAndRotation(playerEntryPoint.position, playerEntryPoint.rotation);
        }

        // 2. 카메라 모드 설정 (보스전의 위압감을 위해 쿼터뷰 권장)
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetMode(CameraController.CameraMode.QuarterView);
        }

        // 3. 보스 스폰
        SpawnBoss();

        Debug.Log($"[BossRoom] Boss room initialized. NodeId: {nodeId}");
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError("[BossRoom] BossPrefab or SpawnPoint is missing!");
            return;
        }

        GameObject go = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        spawnedBoss = go.GetComponent<EnemyController>();

        if (spawnedBoss != null)
        {
            // 보스 사망 이벤트 구독
            spawnedBoss.OnDead += HandleBossDead;
        }
        else
        {
            Debug.LogError("[BossRoom] Spawned boss has no EnemyController component.");
        }
    }

    private void HandleBossDead(EnemyController boss)
    {
        if (isFinished) return;

        if (boss != null)
            boss.OnDead -= HandleBossDead;

        float clearTime = Time.time - startTime;
        Debug.Log($"[BossRoom] Boss defeated! ClearTime: {clearTime:F2}s");

        // 보스 처치 즉시 클리어 처리 (별도의 보상 키 부여 가능)
        FinishCleared(clearTime: clearTime, kills: 1, rewardKey: "BOSS_CLEAR_DEFAULT");
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (spawnedBoss != null)
        {
            spawnedBoss.OnDead -= HandleBossDead;
        }
    }

    // 보스방은 EndPoint를 사용하지 않으므로 무시
       public override void OnPlayerReachedEndPoint() { }
}
