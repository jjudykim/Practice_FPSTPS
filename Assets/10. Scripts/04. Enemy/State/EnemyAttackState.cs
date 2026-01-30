public class EnemyAttackState : IEnemyState
{
    private readonly EnemyController owner;
    public bool IsForced => false;

    public EnemyAttackState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(false);
        owner.StopMove();
    }

    public void Tick(float dt)
    {
        if (owner.HasTarget() == false)
        {
            owner.ToIdle();
            return;
        }

        if (owner.IsTargetInAttackRange() == false)
        {
            owner.ToChase();
            return;
        }

        if (owner.CanAttack())
        {
            owner.DoAttack();
        }
    }

    public void Exit()
    {
    }
}