using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour, IDamageable
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

    [Header("Attack")] [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 5.0f;
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

    private bool deathHandled = false;

    private Collider[] cachedColliders;

    public bool IsAlive => curHp > 0;
    public bool IsDead => curHp <= 0;

    public int MaxHp => maxHp;
    public int CurHp => curHp;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        cachedColliders = GetComponentsInChildren<Collider>(true);

        maxHp = Mathf.Max(1, maxHp);
        curHp = Mathf.Clamp(curHp, 0, maxHp);
        
        attackDamage = Mathf.Max(0, attackDamage);

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
        if (agent == null) 
            return;

        if (agent.enabled == false)
            return;

        if (agent.isOnNavMesh == false)
            return;
        
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
        
        agent.SetDestination(worldPos);
    }
    
    public Vector3 PickRandomPatrolPoint()
    {
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        Vector3 candidate = new Vector3(spawnPos.x + r.x, spawnPos.y, spawnPos.z + r.y);

        // NavMesh 위로 보정
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return transform.position;
    }
    
    
    // ===========================
    // 공격
    // ===========================
    public bool CanAttack() => attackTimer <= 0f;

    public void DoAttack()
    {
        attackTimer = attackCooldown;

        if (target == null)
        {
            Debug.LogWarning("[Enemy] DoAttack skipped: target is null");
            return;
        }
        
        if (Managers.Instance.Combat == null)
        {
            Debug.LogWarning("[Enemy] DoAttack skipped: CombatSystem.Instance is null");
            return;
        }
        
        Vector3 hitPoint = target.position;
        Vector3 hitNormal = (target.position - transform.position).normalized;

        bool dealt = Managers.Instance.Combat.TryDealDamage(
            attacker: gameObject,
            victimGO: target.gameObject,
            damage: attackDamage,
            hitPoint: hitPoint,
            hitNormal: hitNormal,
            hitPart: HitPart.Body,
            source: "EnemyMelee"
        );

        Debug.Log($"[Enemy] Attack! target={target.name}, dmg={attackDamage}, dealt={dealt}");
    }
    

    // ===========================
    // 피격/죽음
    // ===========================
    public void ApplyDamage(DamageInfo info)
    {
        if (IsDead)
            return;

        int dmg = Mathf.CeilToInt(Mathf.Max(0f, info.Damage));
        if (dmg <= 0)
            return;

        ApplyDamage(dmg);
    }
    
    public void ApplyDamage(int damage)
    {
        if (IsDead)
            return;

        curHp -= damage;
        curHp = Mathf.Clamp(curHp, 0, maxHp);

        Debug.Log($"[Enemy] Hit! damage={damage}, hp={curHp}/{maxHp}");

        if (curHp <= 0)
        {
            HandleDeathOnce();
            ToDead();
        }
        else
        {
            ToHit();
        }
    }

    private void HandleDeathOnce()
    {
        if (deathHandled)
            return;

        deathHandled = true;
        
        StopMove();
        if (agent != null)
            agent.enabled = false;
        
        if (cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                    cachedColliders[i].enabled = false;
            }
        }

        Debug.Log("[Enemy] ::: Death handled (colliders off, agent off)");
    }
}
