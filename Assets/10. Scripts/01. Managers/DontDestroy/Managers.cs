using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Managers : SingletonBase<Managers>
{
    public GameManager Game { get; private set; }
    public InputManager Input { get; private set; }
    public SaveDataManager SaveData { get; private set; }
    public ExSceneManager Scene { get; private set; }
    public CombatManager Combat { get; private set; }
    public FloatingTextManager FloatingText { get; private set; }
    
    protected override bool AllowAutoCreate => true;
    protected override void OnInitialize()
    {
        base.OnInitialize();

        Game = new GameManager();
        Input = new InputManager();
        SaveData = new SaveDataManager();
        Combat = new CombatManager();
        Scene = new ExSceneManager();
        FloatingText = new FloatingTextManager();
        
        Scene.Init();
        FloatingText.Init();
        Game.Init();
        
        Game.Awake();
    }
    
    private void Update()
    {
        Input.Update();
    }
}