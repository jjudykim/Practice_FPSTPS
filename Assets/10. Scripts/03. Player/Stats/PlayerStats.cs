using System;
using jjudy;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    [Header("Hp")] public int MaxHp = 100;

    [Header("Stamina")] 
    public int MaxStamina = 100;
    public float StaminaRegenRate = 2f;    // per sec

    [Header("Move")] 
    public float MoveSpeed = 5f;
    public float RunSpeedMultiplier = 1.5f;

    [Header("Stamina Costs")] 
    public float RunStaminaCostPerSec = 3f;
    public float DodgeStaminaCost = 10f;

    [Header("Carry Weight")] 
    public float CarryWeight = 40f;
    public float CarryWeightBonus = 0f;
}
    