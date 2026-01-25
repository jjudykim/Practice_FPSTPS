using jjudy;
using Unity.Android.Gradle;
using UnityEngine;

public class PlayerStatsEffecter
{
    public PlayerStatsSO BaseStatsesSo;
    
    [Header("Stats - Base")]
    public PlayerStats Base = new PlayerStats();

    [Header("Stats - Bonus (Equip/Buff)")]
    public PlayerStats Bonus = new PlayerStats
    {
        MaxHp = 0,
        MaxStamina = 0,
        StaminaRegenRate = 0f,
        MoveSpeed = 0f,
        RunSpeedMultiplier = 0f,
        RunStaminaCostPerSec = 0f,
        DodgeStaminaCost = 0f,
        CarryWeight = 0f,
        CarryWeightBonus = 0f
    };

    [Header("Runtime")]
    public PlayerResources Resources = new PlayerResources();

    public ObservableIntValue MaxHpObs { get; private set; } = new ObservableIntValue();
    public ObservableIntValue MaxStaminaObs { get; private set; } = new ObservableIntValue();

    // ============================
    // Effective Stats (Base + Bonus)
    // ============================
    public int MaxHp => MaxHpObs.Value;
    public int MaxStamina => MaxStaminaObs.Value;
    public float StaminaRegenRate => Mathf.Max(0f, Base.StaminaRegenRate + Bonus.StaminaRegenRate);
    public float MoveSpeed => Mathf.Max(0f, Base.MoveSpeed + Bonus.MoveSpeed);
    public float RunSpeedMultiplier => Mathf.Max(0.1f, Base.RunSpeedMultiplier + Bonus.RunSpeedMultiplier);
    public float RunStaminaCostPerSec => Mathf.Max(0f, Base.RunStaminaCostPerSec + Bonus.RunStaminaCostPerSec);
    public float DodgeStaminaCost => Mathf.Max(0f, Base.DodgeStaminaCost + Bonus.DodgeStaminaCost);
    public float CarryWeightLimit => Mathf.Max(0f, Base.CarryWeight + Bonus.CarryWeight + Base.CarryWeightBonus + Bonus.CarryWeightBonus
    );

    private float staminaRegenAccumulator = 0f;
    private float runStaminaConsumeAccumulator = 0f;

    public void InitRuntime()
    {
        ApplyBaseFromSO();
        
        RecalculateEffectiveStats();
        Resources.Init(MaxHp, MaxStamina);
    }

    private void ApplyBaseFromSO()
    {
        if (BaseStatsesSo == null)
            return;

        var src = BaseStatsesSo.BaseStats;

        Base.MaxHp = src.MaxHp;
        
        Base.MaxStamina = src.MaxStamina;
        Base.StaminaRegenRate = src.StaminaRegenRate;
        
        Base.MoveSpeed = src.MoveSpeed;
        Base.RunSpeedMultiplier = src.RunSpeedMultiplier;

        Base.RunStaminaCostPerSec = src.RunStaminaCostPerSec;
        Base.DodgeStaminaCost = src.DodgeStaminaCost;
        
        Base.CarryWeight = src.CarryWeight;
        Base.CarryWeightBonus = src.CarryWeightBonus;
    }
    
    public void RecalculateEffectiveStats()
    {
        int nextMaxHp = Mathf.Max(1, Base.MaxHp + Bonus.MaxHp);
        int nextMaxStamina = Mathf.Max(0, Base.MaxStamina + Bonus.MaxStamina);

        MaxHpObs.Value = nextMaxHp;
        MaxStaminaObs.Value = nextMaxStamina;
        
        ClampRuntimeToMax();
    }

    public bool TickRunStaminaConsume(float dt)
    {
        if (Resources.IsDead)
            return false;

        if (RunStaminaCostPerSec <= 0f)
            return true;

        if (Resources.CurStamina.Value <= 0)
            return false;

        runStaminaConsumeAccumulator += RunStaminaCostPerSec * dt;

        int consumeAmount = Mathf.FloorToInt(runStaminaConsumeAccumulator);

        if (consumeAmount <= 0)
            return true;

        bool success = Resources.TryConsumeStamina(consumeAmount);

        if (success)
        {
            runStaminaConsumeAccumulator -= consumeAmount;
            return true;
        }
        else
        {
            runStaminaConsumeAccumulator = 0f;
            return false;
        }
    }

    public void TickStaminaRegen(float dt)
    {
        if (Resources.IsDead) 
            return;
        
        if (StaminaRegenRate <= 0f) 
            return;

        if (Resources.CurStamina.Value >= MaxStamina)
        {
            staminaRegenAccumulator = 0f;
            return;
        }

        staminaRegenAccumulator += StaminaRegenRate * dt;

        int regenAmount = Mathf.FloorToInt(staminaRegenAccumulator);

        if (regenAmount <= 0)
            return;
        
        Resources.RestoreStamina(regenAmount, MaxStamina);

        staminaRegenAccumulator -= regenAmount;
    }
    
    public void ClampRuntimeToMax()
    {
        Resources.CurHp.Value = Mathf.Min(Resources.CurHp.Value, MaxHp);
        Resources.CurStamina.Value = Mathf.Min(Resources.CurStamina.Value, MaxStamina);
    }        
}

