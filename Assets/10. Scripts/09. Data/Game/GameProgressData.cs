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
public class InventoryData
{
    public int maxSlots = 20;
   public List<ItemStack> items = new List<ItemStack>();
   public List<string> equippedItemIds = new List<string>();

   public event Action OnInventoryChanged;
   
   public static InventoryData CreateNew()
   {
      return new InventoryData()
      {
          maxSlots = 20,
         items = new List<ItemStack>(),
         equippedItemIds = new List<string>()
      };
   }

   public bool AddItem(string itemId, int amount)
   {
      if (string.IsNullOrEmpty(itemId) || amount <= 0)
         return false;

      var itemData = Databases.Instance.Item.GetOrNull(itemId);
      if (itemData == null)
          return false;
      
      // 1. 기존에 스택에 추가 시도
      foreach (var stack in items.Where(x => x.itemId == itemId && !x.IsFull))
      {
          int canAdd = itemData.StackLimit - stack.amount;
          int toAdd = Mathf.Min(canAdd, amount);
          stack.amount += toAdd;
          amount -= toAdd;

          if (amount <= 0)
              break;
      }

      // 2. 새 슬롯 생성 시도
      while (amount > 0)
      {
          if (items.Count >= maxSlots)
          {
              Debug.LogWarning("[Inventory] ::: No more slots available");
              OnInventoryChanged?.Invoke();
              return false;
          }
          
          int toAdd = Mathf.Min(amount, itemData.StackLimit);
          items.Add(new ItemStack(itemId, toAdd));
          amount -= toAdd;
      }

      OnInventoryChanged?.Invoke();
      return true;
   }

   public bool TryRemoveItem(string itemId, int amount)
   {
      if (string.IsNullOrEmpty(itemId) || amount <= 0)
         return true;

       // 전체 수량 확인
       int totalCount = items.Where(x => x.itemId == itemId).Sum(x => x.amount);
       if (totalCount < amount)
           return false;

       for (int i = items.Count - 1; i >= 0; i--)
       {
           if (items[i].itemId == itemId)
           {
               int toRemove = Mathf.Min(items[i].amount, amount);
               items[i].amount -= toRemove;
               amount -= toRemove;

               if (items[i].amount <= 0)
                   items.RemoveAt(i);

               if (amount <= 0)
                   break;
           }
       }
       
       OnInventoryChanged?.Invoke();
       return true;
   }

   public void SortItems()
   {
       items = items
           .OrderBy(x => x.Data.Type)
           .ThenBy(x => x.Data.DisplayName)
           .ToList();
       OnInventoryChanged?.Invoke();
   }
}

[Serializable]
public class ItemStack
{
    public string itemId;
    public int amount;

    public ItemData Data => Databases.Instance.Item.GetOrNull(itemId);
    
    public ItemStack() { }
    
    public ItemStack(string id, int amount = 1)
    {
        itemId = id;
        this.amount = amount;
    }
    
    public bool IsFull => Data != null && amount >= Data.StackLimit;
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