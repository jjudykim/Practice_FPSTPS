using System.Collections.Generic;
using UnityEngine;

public class CombatRoomController : RoomControllerBase
{
    [Header("Room Points")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private EndPointTrigger endPoint;
 
    [Header("Enemy Spawn Points (pre-placed in scene)")]
    [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("Enemy Prefabs (4-5 types, random pick)")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>(); // 4종 넣기
    
    [Header("Options")]
    [SerializeField] private bool shuffleSpawnPoints = false;
    
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private int aliveEnemyCount = 0;
    private int killCount = 0;
    private float elapsed = 0f;

    private void Awake()
    {
        if (Player.Instance != null)
        {
            var combat = Player.Instance.GetComponent<PlayerCombatController>();
            if (combat != null)
            {
                combat.InitializeCombatState();
            }
        }
    }

    public override void Init(int nodeId)
    {
        base.Init(nodeId);
        
        elapsed = 0f;
        killCount = 0;
        aliveEnemyCount = 0;
        spawnedEnemies.Clear();

        ValidateSetup();

        // 1) 플레이어를 EntryPoint로 배치
        PlacePlayerAtEntry();

        // 2) 적 스폰(미리 정해둔 위치)
        SpawnEnemies();

        // 3) EndPoint 비활성으로 시작
        SetEndPointActive(false);
        
        // 4) 카메라 모드 쿼터뷰로 기본 설정
        CameraController.Instance.SetMode(CameraController.CameraMode.QuarterView);

        if (Player.Instance != null)
        {
            var combat = Player.Instance.GetComponent<PlayerCombatController>();
            if (combat != null)
            {
                combat.InitializeCombatState();
            }
        }

        Debug.Log($"[CombatRoomController] Init completed. nodeId={NodeId}, enemyCount={aliveEnemyCount}");
    }

    private void Start()
    {
        if (Player.Instance != null)
        {
            var combat = Player.Instance.GetComponent<PlayerCombatController>();
            if (combat != null)
                combat.InitializeCombatState();
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isFinished)
            return;

        elapsed += Time.deltaTime;
    }
    
    private void ValidateSetup()
    {
        if (entryPoint == null)
            Debug.LogError("[CombatRoomController] EntryPoint is not assigned.");

        if (endPoint == null)
            Debug.LogError("[CombatRoomController] EndPointTrigger is not assigned.");

        if (enemySpawnPoints == null || enemySpawnPoints.Count == 0)
            Debug.LogError("[CombatRoomController] EnemySpawnPoints are empty.");

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            Debug.LogError("[CombatRoomController] EnemyPrefabs are empty.");
    }
    
    
    // ===========================
    // Step 1) Player를 EntryPoint로 이동
    // ===========================
    private void PlacePlayerAtEntry()
    {
        if (entryPoint == null)
            return;

        GameObject player = Player.Instance.gameObject;
        if (player == null)
        {
            Debug.LogError("[CombatRoomController] Player not found.");
            return;
        }
        
        player.transform.position = entryPoint.position;
        player.transform.rotation = entryPoint.rotation;

        Debug.Log($"[CombatRoomController] Player placed at EntryPoint. nodeId={NodeId}");
    }

    // ===========================
    // Step 2) Enemy 스폰
    // - 미리 정해둔 위치에 Instantiate
    // - 프리팹 4종 중 랜덤
    // ===========================
    private void SpawnEnemies()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Count == 0)
            return;

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            return;

        List<Transform> points = new List<Transform>(enemySpawnPoints);
        if (shuffleSpawnPoints)
            Shuffle(points);
        
        for (int i = 0; i < points.Count; i++)
        {
            Transform sp = points[i];
            if (sp == null)
                continue;

            GameObject prefab = PickRandomEnemyPrefab();
            if (prefab == null)
                continue;

            GameObject enemyGO = Instantiate(prefab, sp.position, sp.rotation);

            // EnemyController 이벤트 기반 섬멸 체크
            EnemyController enemy = enemyGO.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogWarning($"[CombatRoomController] Spawned enemy has no EnemyController. name={enemyGO.name}");
                continue;
            }
            
            if (enemy.IsDead)
                continue;

            enemy.OnDead += HandleEnemyDead;

            spawnedEnemies.Add(enemyGO);
            aliveEnemyCount++;
        }
    }

    private GameObject PickRandomEnemyPrefab()
    {
        int idx = Random.Range(0, enemyPrefabs.Count);
        return enemyPrefabs[idx];
    }
    
    private void Shuffle(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            Transform tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    // ===========================
    // Step 3) 적 전멸 시 EndPoint 활성화
    // ===========================
    private void HandleEnemyDead(EnemyController deadEnemy)
    {
        if (isFinished)
            return;
        
        if (deadEnemy != null)
            deadEnemy.OnDead -= HandleEnemyDead;

        killCount++;
        aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);

        Debug.Log($"[CombatRoomController] Enemy dead. alive={aliveEnemyCount}, kills={killCount}");

        if (aliveEnemyCount == 0)
        {
            SetEndPointActive(true);
        }
    }

    private void SetEndPointActive(bool active)
    {
        if (endPoint == null)
            return;

        endPoint.SetActive(active);

        Debug.Log($"[CombatRoomController] EndPoint Active = {active}");
    }

    // ===========================
    // Step 4) EndPoint Trigger 시 룸 클리어
    // ===========================
    public override void OnPlayerReachedEndPoint()
    {
        if (isFinished)
            return;

        // 아직 적이 남아있으면 무시
        if (aliveEnemyCount > 0)
        {
            Debug.Log("[CombatRoomController] Player reached EndPoint but enemies remain. Ignored.");
            return;
        }

        Debug.Log($"[CombatRoomController] Player escaped room! nodeId={NodeId}");

        // 룸 종료를 “클리어”로 처리
        FinishCleared(clearTime: 0f, kills: spawnedEnemies.Count, rewardKey: "COMBAT_DEFAULT");
    }
    
    public void OnPlayerDead()
    {
        if (isFinished)
            return;

        FinishFailed();
    }
    
    private void OnDestroy()
    {
        // 씬 종료 시 이벤트 정리
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                var enemy =  spawnedEnemies[i].GetComponent<EnemyController>();
                enemy.OnDead -= HandleEnemyDead;
            }
        }
    }
}