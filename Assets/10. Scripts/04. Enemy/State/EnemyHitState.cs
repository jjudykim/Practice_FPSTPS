using UnityEngine;

public class EnemyHitState : IEnemyState
{
    private readonly EnemyController owner;
    private float stunTimer;

    public bool IsForced => true;

    public EnemyHitState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(true);
        owner.StopMove();

        stunTimer = 0.2f;
        Debug.Log("[Enemy] ::: Enter Hit");
    }

    public void Tick(float dt)
    {
        if (owner.IsDead)
        {
            owner.ToDead();
            return;
        }

        stunTimer -= dt;
        if (stunTimer <= 0f)
        {
            owner.SetLock(false);
            owner.ToDetect();
        }
    }


    public void Exit()
    {
        Debug.Log("[Enemy] ::: Exit Hit");
    }
}