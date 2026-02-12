using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class GameProgressData
{
    public string playerName = "New Player";
    
    public CurrencyData currency = new CurrencyData();
    public PlayerGrowthData growth = new PlayerGrowthData();
    public StageProgressData stage = new StageProgressData();
    public StorageInventory inventory = new();
    public QuestProgressData quest = new QuestProgressData();
    
    public static GameProgressData CreateNew()
    {
       return new GameProgressData
       {
          currency = CurrencyData.CreateNew(),
          growth = PlayerGrowthData.CreateNew(),
          stage = StageProgressData.CreateNew(),
          inventory = StorageInventory.CreateNew(),
          quest = QuestProgressData.CreateNew()
       };
    }
}

[Serializable]
public class CurrencyData
{
   public int gold = 0;
   public event Action<int> OnGoldChanged;

   public static CurrencyData CreateNew()
   {
      return new CurrencyData { gold = 0 };
   }

   public void AddGold(int amount)
   { 
       gold = Mathf.Max(0, gold + amount);
       OnGoldChanged?.Invoke(gold);
   }

   public bool TrySpendGold(int amount)
   {
       if (gold < amount) 
           return false;
       
       gold -= amount;
       OnGoldChanged?.Invoke(gold);
       return true;
   }
}

[Serializable]
public class PlayerGrowthData
{
   public int level = 1;
   public int exp = 0;

   public event Action<int, int> OnExpChanged;
   public event Action<int> OnLevelUp;

   public int GetRequiredExp() => level * 100;
   
   public static PlayerGrowthData CreateNew()
   {
      return new PlayerGrowthData { level = 1, exp = 0 };
   }

   public void SetLevel(int newLevel)
   {
      level = Mathf.Max(1, newLevel);
   }

   public void AddExp(int amount)
   {
       exp += amount; int required = GetRequiredExp(); 
       while (exp >= required)
       {
           exp -= required;
           level++;
           required = GetRequiredExp();
           OnLevelUp?.Invoke(level);
           Debug.Log($"[Growth] Level Up! Current Level: {level}");
       } 
       OnExpChanged?.Invoke(exp, required);
   }
}

[Serializable]
public class StageProgressData
{
   public int stageIndex = 0;

   public static StageProgressData CreateNew()
   {
      return new StageProgressData { stageIndex = 0 };
   }

   public void SetStage(int stage)
   {
      stageIndex = Mathf.Max(0, stage);
   }

   public void SetProgress(int chapter, int stage)
   {
      stageIndex = Mathf.Max(0, stage);
   }
}


[Serializable]
public class QuestProgressData
{
   public List<QuestState> quests = new List<QuestState>();

   public static QuestProgressData CreateNew()
   {
      return new QuestProgressData
      {
         quests = new List<QuestState>()
      };
   }

   public QuestState GetOrCreate(string questId)
   {
      if (string.IsNullOrEmpty(questId))
         return null;

      QuestState q = quests.Find(x => x != null && x.questId == questId);
      if (q != null)
         return q;

      q = new QuestState { questId = questId, status = QuestStatus.NotStarted, currentValue = 0, targetValue = 0 };
      quests.Add(q);
      return q;
   }
}

public enum QuestStatus
{
   NotStarted = 0,
   InProgress = 1,
   Completed = 2,
   Claimed = 3
}

[Serializable]
public class QuestState
{
   public string questId = "";
   public QuestStatus status = QuestStatus.NotStarted;
   public int currentValue = 0;
   public int targetValue = 0;
}