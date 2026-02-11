using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class MapListManager : MonoBehaviour
{
    [Header("MapList File (persistent DataPath)")]
    private const string mapListFile = "MapList.json";
     
    private MapListData cachedList;

    private string GetMapListPath() => Path.Combine(Application.persistentDataPath, mapListFile);

    public void InvalidateCache()
    {
        cachedList = null;
    }

    public IEnumerator LoadMapList(Action<bool> onDone)
    {
        if (cachedList != null && cachedList.entries != null && cachedList.entries.Count > 0)
        {
            onDone?.Invoke(true);
            yield break;
        }

        // 1. Persistent 먼저 시도
        string filePath = Path.Combine(Application.persistentDataPath, mapListFile);
        
        // 2. 없으면 StreamingAssets 시도
        if (File.Exists(filePath) == false)
        {
            filePath = Path.Combine(Application.streamingAssetsPath, mapListFile);
        }

        if (File.Exists(filePath) == false)
        {
            cachedList = new MapListData();
            onDone?.Invoke(true);
            yield break;
        }

        var data = JsonReader.Load<MapListData>(filePath);
        if (data == default)
        {
            Debug.LogError($"[MapListManager] MapList load failed. filePath = {filePath}");
            cachedList = null;
            onDone?.Invoke(false);
            yield return null;
        }
        else
        {
            cachedList = data;
            onDone?.Invoke(true);
        }
    }

    public IEnumerator PickRandomPreset(Action<bool, MapData> onDone)
    {
        bool listOk = false;
        yield return LoadMapList(ok => listOk = ok);

        if (listOk == false || cachedList == null || cachedList.entries == null || cachedList.entries.Count == 0)
        {
            onDone?.Invoke(false, null);
            yield break;
        }

        int idx = UnityEngine.Random.Range(0, cachedList.entries.Count);
        MapListEntry entry = cachedList.entries[idx];

        if (entry == null || string.IsNullOrEmpty(entry.presetFile))
        {
            Debug.LogError("[MapListManager] Selected entry is Invalid");
            onDone?.Invoke(false, null);
            yield break;
        }

        // 경로 처리 Fallback
        string fullPath = entry.presetFile;
        if (File.Exists(fullPath) == false)
        {
            // 상대경로일 경우 처리
            fullPath = Path.Combine(Application.persistentDataPath, entry.presetFile);
            if (File.Exists(fullPath) == false)
            {
                fullPath = Path.Combine(Application.streamingAssetsPath, entry.presetFile);
            }
        }

        var preset = JsonReader.Load<MapData>(fullPath);
        if (preset == default)
        {
            Debug.LogError($"[MapListManager] Preset Load Failed: {fullPath}");
            onDone?.Invoke(false, null);
            yield return null;
        }
        else
        {
            onDone?.Invoke(true, preset);
        }
    }
}
