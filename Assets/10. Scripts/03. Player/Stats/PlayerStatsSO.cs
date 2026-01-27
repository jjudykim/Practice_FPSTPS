using UnityEngine;

[CreateAssetMenu(menuName = "Player/Stats", fileName = "PlayerStatsSO")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("BaseStats")] 
    public PlayerStats BaseStats = new PlayerStats();
}