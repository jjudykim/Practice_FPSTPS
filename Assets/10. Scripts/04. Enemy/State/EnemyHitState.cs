using UnityEngine;

public class EnemyHitState : IEnemyState
{
    private readonly EnemyController owner;

    private float hitTimer;
    private const float HIT_MIN_DURATION = 0.5f;

    public bool IsForced => true;

    public EnemyHitState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(true);
        owner.StopMove();

        owner.AnimTriggerDamage();
        owner.SetChaseSpeed(false);
        hitTimer = HIT_MIN_DURATION;
    }

    public void Tick(float dt)
    {
        if (owner.IsDead)
        {
            owner.ToDead();
            return;
        }

        hitTimer -= dt;
        if (hitTimer > 0f)
            return;

        owner.SetLock(false);
        
        if (owner.HasTarget() == false)
        {
            owner.ToIdle();
            return;
        }

        if (owner.IsTargetInAttackRange())
        {
            owner.ToAttack();
            return;
        }

        if (owner.IsTargetInDetectRange())
        {
            owner.ToChase();
            return;
        }

        owner.ToIdle();
    }


    public void Exit()
    {
        owner.ResumeMove();
    }
}