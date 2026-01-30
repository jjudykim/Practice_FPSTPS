using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;

    [Header("Stats")] 
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int curHp = 30;

    [Header("Ranges")] 
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float attackRange = 2.2f;

    [Header("Attack")] [SerializeField] private float attackCooldown = 1.0f;
    private float attackTimer = 0f;

    // 스폰 기준점
    private Vector3 spawnPos;
    
    // FSM
    private EnemyStateMachine fsm;
    
    // States
    private EnemyIdleState stateIdle;
    private EnemyDetectState stateDetect;
    private EnemyChaseState stateChase;
    private EnemyAttackState stateAttack;
    private EnemyHitState stateHit;
    private EnemyDeadState stateDead;

    public bool IsDead => curHp <= 0;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        spawnPos = transform.position;

        fsm = new EnemyStateMachine();

        stateIdle = new EnemyIdleState(this);
        stateDetect = new EnemyDetectState(this);
        stateChase = new EnemyChaseState(this);
        stateAttack = new EnemyAttackState(this);
        stateHit = new EnemyHitState(this);
        stateDead = new EnemyDeadState(this);
        
        // NavMesh Agent 기본 세팅
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.stoppingDistance = 0.0f;
    }

    private void Start()
    {
        fsm.ChangeState(stateIdle);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (IsDead)
        {
            if (fsm.CurrentState != stateDead)
                fsm.ChangeState(stateDead);

            fsm.Tick(dt);
            return;
        }

        if (attackTimer > 0f)
            attackTimer -= dt;

        fsm.Tick(dt);
    }
    
    // ==============================
    //        상태 전이 래퍼
    // ==============================
    public void ToIdle() => fsm.ChangeState(stateIdle);
    public void ToDetect() => fsm.ChangeState(stateDetect);
    public void ToChase() => fsm.ChangeState(stateChase);
    public void ToAttack() => fsm.ChangeState(stateAttack);
    public void ToHit() => fsm.ChangeState(stateHit);
    public void ToDead() => fsm.ChangeState(stateDead);

    public void SetLock(bool locked) => fsm.SetLock(locked);
    
    // ==============================
    //        Target / Range
    // ==============================
    public bool HasTarget() => target != null;
    public Transform GetTargetTransform() => target;

    public float DistanceToTarget()
    {
        if (target == null)
            return float.MaxValue;

        return Vector3.Distance(transform.position, target.position);
    }

    public bool IsTargetInDetectRange()
    {
        return HasTarget() && DistanceToTarget() <= detectRange;
    }

    public bool IsTargetInAttackRange()
    {
        return HasTarget() && DistanceToTarget() <= attackRange;
    }
    
    // ==============================
    //       NaveMesh 이동 유틸
    // ==============================
    public void StopMove()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    public void ResumeMove()
    {
        if (agent == null) return;
        agent.isStopped = false;
    }

    public void MoveTo(Vector3 worldPos)
    {
        if (agent == null) return;

        // isStopped 상태면 목적지 지정해도 안 움직이니, 호출 측에서 ResumeMove 보장
        agent.SetDestination(worldPos);
    }
    
    public Vector3 PickRandomPatrolPoint()
    {
        // 스폰 기준 랜덤 점
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        Vector3 candidate = new Vector3(spawnPos.x + r.x, spawnPos.y, spawnPos.z + r.y);

        // NavMesh 위로 보정
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // 실패하면 현재 위치
        return transform.position;
    }
    
    
    // ===========================
    // 공격
    // ===========================
    public bool CanAttack() => attackTimer <= 0f;

    public void DoAttack()
    {
        Debug.Log($"[Enemy] Attack! target={ (target != null ? target.name : "null") }");
        attackTimer = attackCooldown;
    }
    

    // ===========================
    // 피격/죽음
    // ===========================
    public void ApplyDamage(int damage)
    {
        if (IsDead)
            return;

        curHp -= damage;
        Debug.Log($"[Enemy] Hit! damage={damage}, hp={curHp}/{maxHp}");

        if (curHp <= 0)
        {
            curHp = 0;
            ToDead();
        }
        else
        {
            ToHit();
        }
    }
}
