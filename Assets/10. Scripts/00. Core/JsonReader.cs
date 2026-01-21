using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonReader
{
    public static T Load<T>(string filePath)
    {
        string fullPath = filePath.EndsWith(".json") ? filePath : $"{filePath}.json";

        if (File.Exists(fullPath) == false)
        {
            Debug.LogWarning($"[JsonLoader] File Not Found: {filePath}");
            return default;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            T data = JsonConvert.DeserializeObject<T>(json);
            Debug.Log($"[JsonLoader] Loaded: {filePath}]");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonLoader] Load Failed : {fullPath}\n{e}");
            return default;
        }
    }
}