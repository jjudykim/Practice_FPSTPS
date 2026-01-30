public class EnemyDetectState : IEnemyState
{
    private readonly EnemyController owner;
    public bool IsForced => false;

    public EnemyDetectState(EnemyController owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.SetLock(false);
        owner.ResumeMove();
    }

    public void Tick(float dt)
    {
        if (owner.HasTarget() == false || owner.IsTargetInDetectRange() == false)
        {
            owner.ToIdle();
            return;
        }

        owner.ToChase();
    }

    public void Exit()
    {
    }
}