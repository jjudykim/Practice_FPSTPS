using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Managers : SingletonBase<Managers>
{
    public GameManager Game { get; private set; }
    public InputManager Input { get; private set; }
    public SaveDataManager SaveData { get; private set; }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Game = new GameManager();
        Input = new InputManager();
        SaveData = new SaveDataManager();

        Game.Awake();
    }
    
    private void Update()
    {
        Input.Update();
    }
}