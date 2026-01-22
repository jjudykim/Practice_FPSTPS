using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Managers : SingletonBase<Managers>
{
    public GameManager Game { get; private set; }
    public InputManager Input { get; private set; }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Game = new GameManager();
        Input = new InputManager();

        Game.Awake();
    }
    
    private void Update()
    {
        Input.Update();
    }
}