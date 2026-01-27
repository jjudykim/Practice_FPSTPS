using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class GameProgressData
{
   public CurrencyData currency = new CurrencyData();
   public PlayerGrowthData growth = new PlayerGrowthData();
   public StageProgressData stage = new StageProgressData();

   public InventoryData inventory = new InventoryData();
   public QuestProgressData quest = new QuestProgressData();

   public static GameProgressData CreateNew()
   {
      return new GameProgressData
      {
         currency = CurrencyData.CreateNew(),
         growth = PlayerGrowthData.CreateNew(),
         stage = StageProgressData.CreateNew(),
         inventory = InventoryData.CreateNew(),
         quest = QuestProgressData.CreateNew()
      };
   }

   public string GetStageTitleText() => $"Stage {stage.stageIndex}";
}

[Serializable]
public class CurrencyData
{
   public int gold = 0;
   public int gem = 0;

   public static CurrencyData CreateNew()
   {
      return new CurrencyData { gold = 0, gem = 0 };
   }

   public void AddGold(int amount)
   {
      gold += amount;
      if (gold < 0)
         gold = 0;
   }

   public bool TrySpendGold(int amount)
   {
      if (amount <= 0)
         return true;

      if (gold < amount)
         return false;

      gold -= amount;
      return true;
   }
}

[Serializable]
public class PlayerGrowthData
{
   public int level = 1;
   public int exp = 0;

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
      exp += Mathf.Max(0, amount);
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
public class InventoryData
{
   // TODO : 나중에 정의된 Item 타입으로 바꿀 예정
   public List<ItemStack> items = new List<ItemStack>();

   public List<string> equippedItemIds = new List<string>();

   public static InventoryData CreateNew()
   {
      return new InventoryData()
      {
         items = new List<ItemStack>(),
         equippedItemIds = new List<string>()
      };
   }

   public void AddItem(string itemId, int amount)
   {
      if (string.IsNullOrEmpty(itemId) || amount <= 0)
         return;

      int idx = items.FindIndex(x => x != null && x.itemId == itemId);
      if (idx >= 0)
         items[idx].amount += amount;
      else
         items.Add(new ItemStack {itemId = itemId, amount = amount});
   }

   public bool TryRemoveItem(string itemId, int amount)
   {
      if (string.IsNullOrEmpty(itemId) || amount <= 0)
         return true;

      int idx = items.FindIndex(x => x != null && x.itemId == itemId);
      if (idx < 0)
         return false;

      if (items[idx].amount < amount)
         return false;

      items[idx].amount -= amount;
      
      if (items[idx].amount <= 0)
         items.RemoveAt(idx);

      return true;
   }
}

[Serializable]
public class ItemStack
{
   public string itemId = "";
   public int amount = 0;
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