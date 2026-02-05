using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private readonly EnemyController owner;
    private float repathTimer;
    public bool IsForced => false;

    public EnemyChaseState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(false);
        owner.ResumeMove();
        owner.SetChaseSpeed(true);
        
        owner.SetAnimChasing(true);

        repathTimer = 0f;
    }

    public void Tick(float dt)
    {
        if (owner.HasTarget() == false)
        {
            owner.ToIdle();
            return;
        }

        if (owner.IsTargetInDetectRange() == false)
        {
            owner.ToIdle();
            return;
        }

        if (owner.IsTargetInAttackRange())
        {
            owner.ToAttack();
            return;
        }

        repathTimer -= dt;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.1f;

            Transform target = owner.GetTargetTransform();
            owner.MoveTo(target.position);
        }
    }

    public void Exit()
    {
        owner.SetChaseSpeed(false);
        owner.SetAnimChasing(false);
    }
}