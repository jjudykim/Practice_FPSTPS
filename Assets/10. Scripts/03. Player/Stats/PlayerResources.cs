using System;
using UnityEngine;
using jjudy;

[Serializable]
public class PlayerResources
{
    [Header("Runtime Resources (Observable)")]
    public ObservableIntValue CurHp = new ObservableIntValue();
    public ObservableIntValue CurStamina = new ObservableIntValue();
    public event Action OnDead;

    public void Init(int startHp, int startStamina)
    {
        CurHp.Value = Mathf.Max(0, startHp);
        CurStamina.Value = Mathf.Max(0, startStamina);
    }

    public bool IsDead => CurHp.Value <= 0;

    public void ApplyDamage(int damage)
    {
        if (IsDead) return;

        damage = Mathf.Max(0, damage);
        CurHp.Value = Mathf.Max(0, CurHp.Value - damage);

        if (CurHp.Value <= 0)
            OnDead?.Invoke();
    }

    public void Heal(int amount, int maxHp)
    {
        if (IsDead) return;

        amount = Mathf.Max(0, amount);
        CurHp.Value = Mathf.Min(maxHp, CurHp.Value + amount);
    }

    public bool TryConsumeStamina(int cost)
    {
        cost = Mathf.Max(0, cost);

        if (CurStamina.Value < cost)
            return false;

        CurStamina.Value = Mathf.Max(0, CurStamina.Value - cost);
        return true;
    }

    public void RestoreStamina(int amount, int maxStamina)
    {
        amount = Mathf.Max(0, amount);
        CurStamina.Value = Mathf.Min(maxStamina, CurStamina.Value + amount);
    }
}