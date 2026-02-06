using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPrefabRegistry", menuName = "jjudySO/Weapon/PrefabRegistry")]
public class WeaponPrefabRegistry : ScriptableObject
{ 
    [Serializable]
    public struct WeaponEntry
    { 
        public string Id;
        public WeaponBase Prefab;
    }

    [SerializeField] private List<WeaponEntry> entries = new List<WeaponEntry>();
    private Dictionary<string, WeaponBase> registryDict;

    public void Initialize()
    {
        registryDict = new Dictionary<string, WeaponBase>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            if (!registryDict.ContainsKey(entry.Id))
                registryDict.Add(entry.Id, entry.Prefab);
        }
    }

    public WeaponBase GetPrefab(string id)
    {
        if (registryDict == null) Initialize();
            return registryDict.TryGetValue(id, out var prefab) ? prefab : null;
    }
}