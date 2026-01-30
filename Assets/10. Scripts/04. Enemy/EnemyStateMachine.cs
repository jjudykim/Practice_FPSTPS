public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }
    
    // 피격/죽음 같은 강제 상태에서 다른 상태로 전이되지 못하도록 잠금
    public bool IsLocked { get; private set; }

    public void Tick(float dt)
    {
        CurrentState.Tick(dt);
    }

    public void ChangeState(IEnemyState next)
    {
        if (next == null)
            return;

        if (IsLocked && next.IsForced == false)
            return;

        CurrentState.Exit();
        CurrentState = next;
        CurrentState.Enter();
    }

    public void SetLock(bool locked)
    {
        IsLocked = locked;
    }
}

public interface IEnemyState
{
    bool IsForced { get; }
    void Enter();
    void Tick(float dt);
    void Exit();
}