using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform floatingTextPivot;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Stats")] 
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int curHp = 30;

    [Header("Ranges")] 
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float attackRange = 2.2f;

    [Header("Attack")] 
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 5.0f;
    private float attackTimer = 0f;
    
    // ===========================
    // Chase Speed
    // ===========================
    [Header("Chase Speed")]
    [SerializeField] private float chaseSpeedMultiplier = 1.35f;
    private float baseAgentSpeed = -1f;

    // ===========================
    // Elite Boost
    // ===========================
    [Header("Elite Boost")]
    [SerializeField] private float eliteHpMultiplier = 4.0f;
    [SerializeField] private float eliteDamageMultiplier = 2.0f;
    [SerializeField] private float eliteChaseSpeedMultiplier = 1.15f; // Cha

    // ===========================
    // Ranged Projectile
    // ===========================
    [Header("Ranged Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform projectileMuzzle;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;
    
    // ===========================
    // Aim / Facing
    // ===========================
    [Header("Aim / Facing")]
    [SerializeField] private bool rangedRotateOnEachAttack = true;
    [SerializeField] private bool aimUseTargetHeight = false;
    [SerializeField] private float aimTargetYOffset = 0.5f;
    
    // ===========================
    // Elite Shotgun Pattern 
    // ===========================
    [Header("Elite Shotgun Pattern")]
    [SerializeField] private int elitePelletCount = 3;               
    [SerializeField] private float eliteSpreadAngle = 18f;           
    [SerializeField] private float elitePelletDamageMultiplier = 0.5f; 
    [SerializeField] private bool eliteRandomizeSpread = false;     
    [SerializeField] private float elitePitchJitter = 0f; 
    
    [Header("Drop Rewards")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject expPrefab;
    [SerializeField] private int minGold = 10;
    [SerializeField] private int maxGold = 30;
    [SerializeField] private int expReward = 50;
    [SerializeField] private float dropRadius = 1.5f;
    
    // Attack 트리거
    private bool pendingAttack = false;
    
    // 스폰 기준점
    private Vector3 spawnPos;
    
    
    // =================================
    //               FSM
    // =================================
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

    public event Action<EnemyController> OnDead;
    
    // =================================
    //        Animator Param Hash
    // =================================
    private static readonly int ANIM_SPEED  = Animator.StringToHash("Speed");
    private static readonly int ANIM_IS_CHASING  = Animator.StringToHash("IsChasing");
    private static readonly int ANIM_ATTACK = Animator.StringToHash("Attack");
    private static readonly int ANIM_DAMAGE   = Animator.StringToHash("Damage");
    private static readonly int ANIM_DEAD   = Animator.StringToHash("Dead");
    

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

        target = Player.Instance.transform;

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
        agent.stoppingDistance = attackRange - 1.0f;

        baseAgentSpeed = agent.speed;

        ApplyTypeModifiers();
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

        UpdateAnimatorSpeed(dt);
        
        fsm.Tick(dt);
    }

    private void UpdateAnimatorSpeed(float dt)
    {
        if (animator == null || IsDead)
            return;

        float speed01 = 0f;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            float v = agent.velocity.magnitude;
            float max = Mathf.Max(0.01f, agent.speed);
            speed01 = Mathf.Clamp01(v / max);
        }
        
        animator.SetFloat(ANIM_SPEED, speed01, 0.1f, dt);
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
    //       Animator Wrapper
    // ==============================
    public void SetAnimChasing(bool isChasing)
    {
        if (animator == null)
            return;
        
        animator.SetBool(ANIM_IS_CHASING, isChasing);
    }

    public void AnimTriggerAttack()
    {
        if (animator == null)
            return;
        animator.ResetTrigger(ANIM_ATTACK);
        animator.SetTrigger(ANIM_ATTACK);
    }
    
    public void AnimTriggerDamage()
    {
        if (animator == null)
            return;
        
        animator.ResetTrigger(ANIM_DAMAGE);
        animator.SetTrigger(ANIM_DAMAGE);
    }

    public void AnimTriggerDead()
    {
        if (animator == null)
            return;
        
        animator.ResetTrigger(ANIM_DEAD);
        animator.SetTrigger(ANIM_DEAD);
    }
    
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

    public void SetChaseSpeed(bool enable)
    {
        if (baseAgentSpeed <= 0f)
            baseAgentSpeed = agent.speed;

        float mul = chaseSpeedMultiplier;

        if (enemyType == EnemyType.Elite)
            mul *= eliteChaseSpeedMultiplier;

        agent.speed = enable ? baseAgentSpeed * mul : baseAgentSpeed;
    }

    // ===========================
    //            공격
    // ===========================
    private bool UsesProjectileAttack()
    {
        return enemyType == EnemyType.Ranged || enemyType == EnemyType.Elite;
    }
    
    public bool CanAttack() => attackTimer <= 0f;

    public void RequestAttack()
    {
        if (CanAttack() == false)
            return;

        if (UsesProjectileAttack() && rangedRotateOnEachAttack)
            RotateYawTowardsTarget();
        
        attackTimer = attackCooldown;
        pendingAttack = true;

        AnimTriggerAttack();
    }

    public void AnimEvent_AttackHit()
    {
        if (pendingAttack == false)
            return;

        pendingAttack = false;
        
        if (UsesProjectileAttack() && rangedRotateOnEachAttack)
            RotateYawTowardsTarget();

        if (enemyType == EnemyType.Ranged)
            FireProjectile(attackDamage);
        else if (enemyType == EnemyType.Elite)
            FireEliteShotgun();
        else
            DealMeleeDamageNow();
    }

    private void DealMeleeDamageNow()
    {
        if (target == null)
        {
            Debug.LogWarning("[Enemy] DealAttackDamageNow skipped: target is null");
            return;
        }

        if (Managers.Instance.Combat == null)
        {
            Debug.LogWarning("[Enemy] DealAttackDamageNow skipped: Managers.Instance.Combat is null");
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

        Debug.Log($"[Enemy] AttackHit! target={target.name}, dmg={attackDamage}, dealt={dealt}");
    }
    
    private Vector3 GetMuzzlePosition()
    {
        return projectileMuzzle != null
            ? projectileMuzzle.position
            : (transform.position + Vector3.up * 1.0f);
    }

    private Vector3 GetAimPoint()
    {
        if (target == null)
            return GetMuzzlePosition() + (transform.forward * 5f);

        return target.position + Vector3.up * aimTargetYOffset;
    }
    
    private Vector3 GetBaseFireDirection()
    {
        Vector3 muzzlePos = GetMuzzlePosition();

        if (aimUseTargetHeight && target != null)
        {
            Vector3 to = GetAimPoint();
            Vector3 dir = (to - muzzlePos);
            if (dir.sqrMagnitude < 0.0001f)
                dir = (projectileMuzzle != null ? projectileMuzzle.forward : transform.forward);

            return dir.normalized;
        }

        Vector3 forward = projectileMuzzle != null ? projectileMuzzle.forward : transform.forward;
        if (forward.sqrMagnitude < 0.0001f)
            forward = transform.forward;

        return forward.normalized;
    }
    
    private void FireProjectile(int damage)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[Enemy] FireProjectile skipped: projectilePrefab is null");
            return;
        }

        Vector3 spawn = GetMuzzlePosition();
        Vector3 dir = GetBaseFireDirection();

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        EnemyProjectile proj = Instantiate(projectilePrefab, spawn, rot);
        proj.Init(
            owner: gameObject,
            direction: dir,
            damage: damage,
            speed: projectileSpeed,
            lifeTime: projectileLifeTime
        );
    }
    
    private void FireEliteShotgun()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[Enemy] FireEliteShotgun skipped: projectilePrefab is null");
            return;
        }

        int pelletCount = Mathf.Max(1, elitePelletCount);
        float spread = Mathf.Max(0f, eliteSpreadAngle);

        // Elite 산탄 데미지(탄 1발 당)
        int pelletDamage = Mathf.Max(1, Mathf.RoundToInt(attackDamage * Mathf.Max(0f, elitePelletDamageMultiplier)));

        Vector3 spawn = projectileMuzzle != null
            ? projectileMuzzle.position
            : (transform.position + Vector3.up * 1.0f);

        Vector3 baseDir = projectileMuzzle != null
            ? projectileMuzzle.forward
            : transform.forward;

        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = transform.forward;

        Quaternion baseRot = projectileMuzzle != null
            ? projectileMuzzle.rotation
            : Quaternion.LookRotation(baseDir);

        // 퍼짐 각도 계산
        // pelletCount=1이면 중앙 1발
        for (int i = 0; i < pelletCount; i++)
        {
            float yawOffset;

            if (pelletCount == 1 || spread <= 0f)
            {
                yawOffset = 0f;
            }
            else if (eliteRandomizeSpread)
            {
                yawOffset = Random.Range(-spread * 0.5f, spread * 0.5f);
            }
            else
            {
                float t = (pelletCount == 1) ? 0.5f : (i / (float)(pelletCount - 1));
                yawOffset = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
            }

            float pitchOffset = 0f;
            if (elitePitchJitter > 0f)
            {
                pitchOffset = eliteRandomizeSpread
                    ? Random.Range(-elitePitchJitter, elitePitchJitter)
                    : Mathf.Lerp(-elitePitchJitter, elitePitchJitter, (pelletCount == 1) ? 0.5f : (i / (float)(pelletCount - 1)));
            }
            
            Quaternion offsetRot = Quaternion.Euler(pitchOffset, yawOffset, 0f);
            Quaternion finalRot = baseRot * offsetRot;

            Vector3 finalDir = finalRot * Vector3.forward;

            EnemyProjectile proj = Instantiate(projectilePrefab, spawn, finalRot);
            proj.Init(
                owner: gameObject,
                direction: finalDir,
                damage: pelletDamage,
                speed: projectileSpeed,
                lifeTime: projectileLifeTime
            );
        }

        Debug.Log($"[Enemy] EliteShotgun! pellets={pelletCount}, spread={spread}, pelletDmg={pelletDamage}");
    }

    private void RotateYawTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 from = transform.position;
        Vector3 to = GetAimPoint();

        Vector3 dir = (to - from);
        dir.y = 0f; // ✅ 무조건 수평만

        if (dir.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
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

        if (Managers.Instance.FloatingText != null)
        {
            if (info.HitPart == HitPart.Body)
                Managers.Instance.FloatingText.Show(floatingTextPivot.position, dmg.ToString(), Color.white);
            if (info.HitPart == HitPart.Head)
                Managers.Instance.FloatingText.Show(floatingTextPivot.position, dmg.ToString(), Color.yellow);
        }

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

        SpawnDropItems();

        Debug.Log("[Enemy] ::: Death handled (colliders off, agent off)");
        OnDead?.Invoke(this);

        if (enemyType == EnemyType.Elite)
        {
            Managers.Instance.Game.GameClear();
        }
    }

    private void SpawnDropItems()
    {
        // 1. 골드 생성
        int totalGold = Random.Range(minGold, maxGold);
        if (enemyType == EnemyType.Elite) totalGold *= 5; // 엘리트는 5배
    
        // 골드 프리팹이 있다면 5개 정도로 나눠서 생성
        int goldPieceCount = 3;
        int goldPerPiece = totalGold / goldPieceCount;
    
        for (int i = 0; i < goldPieceCount; i++)
        {
            SpawnSingleItem(goldPrefab, DropItemType.Gold, goldPerPiece);
        }
    
        // 2. 경험치 생성
        int finalExp = enemyType == EnemyType.Elite ? expReward * 10 : expReward;
        SpawnSingleItem(expPrefab, DropItemType.Exp, finalExp);
    }
    
    private void SpawnSingleItem(GameObject prefab, DropItemType type, int val)
    {
        if (prefab == null) return;
    
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * dropRadius;
        Vector3 spawnOffset = new Vector3(randomCircle.x, 0.5f, randomCircle.y);
        Vector3 finalSpawnPos = transform.position + spawnOffset;
        
        GameObject go = Instantiate(prefab, finalSpawnPos, Quaternion.identity);
        var dropItem = go.GetComponent<FieldDropItem>();
        if (dropItem != null)
        {
            dropItem.Init(type, val);
        }
    }

    private void ApplyTypeModifiers()
    {
        if (enemyType != EnemyType.Elite)
            return;
        
        maxHp = Mathf.Max(1, Mathf.CeilToInt(maxHp * eliteHpMultiplier));
        attackDamage = Mathf.Max(0, Mathf.CeilToInt(attackDamage * eliteDamageMultiplier));
        
        curHp = maxHp;
    }
}
