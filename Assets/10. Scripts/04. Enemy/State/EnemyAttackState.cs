using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private readonly EnemyController owner;
    public bool IsForced => false;

    private const float FACE_TURN_SPEED = 720f;

    public EnemyAttackState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(false);
        owner.StopMove();

        owner.SetChaseSpeed(false);
    }

    public void Tick(float dt)
    {
        if (owner.HasTarget() == false)
        {
            owner.ToIdle();
            return;
        }

        FaceTarget(dt);

        if (owner.IsTargetInAttackRange() == false)
        {
            owner.ToChase();
            return;
        }

        if (owner.CanAttack())
        {
            owner.RequestAttack();
        }
    }

    private void FaceTarget(float dt)
    {
        Transform t = owner.GetTargetTransform();
        if (t == null)
            return;

        Vector3 dir = (t.position - owner.transform.position);
        dir.y = 0f;
        
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        owner.transform.rotation = Quaternion.RotateTowards(
            owner.transform.rotation,
            targetRot,
            FACE_TURN_SPEED * dt
        );
    }

    public void Exit()
    {
    }
}