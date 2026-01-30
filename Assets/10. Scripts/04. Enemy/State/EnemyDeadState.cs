using UnityEngine;

public class EnemyDeadState : IEnemyState
{
    private readonly EnemyController owner;
    private float despawnTimer;
    public bool IsForced => true;

    public EnemyDeadState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(true);
        owner.StopMove();

        despawnTimer = 1.0f;
        Debug.Log("[Enemy] ::: Enter Dead");
    }

    public void Tick(float dt)
    {
        despawnTimer -= dt;
        if (despawnTimer <= 0f)
        {
            Object.Destroy(owner.gameObject);
        }
    }

    public void Exit()
    {
    }
}