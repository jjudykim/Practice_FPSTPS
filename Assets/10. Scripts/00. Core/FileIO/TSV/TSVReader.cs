using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public static class TSVReader
{
    private static readonly CsvConfiguration TsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = "\t",
        Mode = CsvMode.NoEscape,
        HasHeaderRecord = true,
        MissingFieldFound = null,
        HeaderValidated = null,
    };

    public static async Task<List<T>> ReadTableAsync<T>(string tableName, bool isStreamingAssetPath = false)
    {
        string basePath = isStreamingAssetPath ? Application.streamingAssetsPath : Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, "Table");
        string filePath = Path.Combine(folderPath, tableName + ".tsv");

        if (File.Exists(filePath) == false)
        {
            Debug.LogError($"[TableLoader] 파일이 존재하지 않습니다: {filePath}");
            return null;
        }

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, TsvConfig);

            var records = new List<T>();
            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                records.Add(record);
            }

            return records;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TableReader] {tableName}.tsv 로딩 실패: {ex.Message}");
            return null;
        }
    }

    public static List<T> ReadTable<T>(string filePath)
    {
        if (File.Exists(filePath) == false)
        {
            Debug.LogError($"[TableReader] 파일이 존재하지 않습니다: {filePath}");
            return null;
        }

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, TsvConfig);

            var records = new List<T>();
            
            foreach (var record in csv.GetRecords<T>())
            {
                records.Add(record);
            }

            return records;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TableLoader] {filePath} 로딩 실패: {ex.Message}");
            return null;
        }
    }
}