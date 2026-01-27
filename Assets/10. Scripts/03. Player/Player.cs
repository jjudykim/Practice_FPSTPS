using System;
using UnityEngine;
using jjudy;
using UnityEngine.Serialization;

public class Player : SingletonBase<Player>
{
    [Header("Runtime Stats")] 
    [SerializeField] private PlayerStatsSO baseStatsSO;
    
    [Header("Runtime Stats")]
    [SerializeField] private PlayerStatsEffecter stats = new PlayerStatsEffecter();

    // 상태 플래그는 Player가 관리
    [Header("State Flags")]
    public bool IsRolling { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAiming { get; set; }
    public bool IsReloading { get; set; }
    public bool IsWeaponEquipped { get; set; }
    
    public PlayerStatsEffecter Stats => stats;

    public bool IsDead => stats.Resources.IsDead;
    public bool CanRun => (IsRolling == false) && stats.Resources.CurStamina.Value > 0;
    public bool CanRoll => (IsRolling == false) && stats.Resources.CurStamina.Value >= stats.DodgeStaminaCost;

    private void Awake()
    {
        stats.BaseStatsesSo = baseStatsSO;
        stats.InitRuntime();
    }

    private void Update()
    {
        if (IsRolling || IsRunning)
            return;

        stats.TickStaminaRegen(Time.deltaTime);
    }
    
    public void ApplyDamage(int damage)
    {
        stats.Resources.ApplyDamage(damage);
    }

    public void Heal(int amount)
    {
        stats.Resources.Heal(amount, stats.MaxHp);
    }

    public bool TryConsumeStaminaForRun(float dt)
    {
        return stats.TickRunStaminaConsume(dt);
    }

    public bool TryConsumeStaminaForDodge()
    {
        int cost = Mathf.CeilToInt(stats.DodgeStaminaCost);
        return stats.Resources.TryConsumeStamina(cost);
    }

    public void OnStatsModifiedByEquipOrBuff()
    {
        stats.RecalculateEffectiveStats();
    }
}