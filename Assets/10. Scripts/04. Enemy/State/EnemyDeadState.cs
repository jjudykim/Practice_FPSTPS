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

        if (owner.GetComponent<Animator>() != null)
        {
            var anim = owner.GetComponent<Animator>();
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Damage");
            anim.SetBool("IsChasing", false);
            anim.SetFloat("Speed", 0f);
        }
        owner.AnimTriggerDead();

        owner.StopMove();
        
        despawnTimer = 2.0f;
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