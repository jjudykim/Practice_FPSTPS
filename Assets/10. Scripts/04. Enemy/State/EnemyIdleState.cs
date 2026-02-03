using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private readonly EnemyController owner;

    private Vector3 patrolDest;
    private float repickTimer;

    public bool IsForced => false;

    public EnemyIdleState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(false);

        owner.ResumeMove();

        repickTimer = 0f;
        patrolDest = owner.transform.position;
    }

    public void Tick(float dt)
    {
        if (owner.IsTargetInDetectRange())
        {
            owner.ToDetect();
            return;
        }

        repickTimer -= dt;
        if (repickTimer <= 0f)
        {
            repickTimer = Random.Range(1.0f, 2.5f);

            patrolDest = owner.PickRandomPatrolPoint();
            owner.MoveTo(patrolDest);
        }
    }

    public void Exit()
    {
        
    }
}