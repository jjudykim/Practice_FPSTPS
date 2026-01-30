using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

public interface ITsvRowWithId
{
    string Id { get; }
}

/// <summary>
/// TSV 기반 공통 데이터베이스 베이스 클래스
/// - EnsureLoadedAsync: 중복 로드 방지
/// - TryGet / GetOrNull: ID 기반 조회
/// - ReloadAsync: 강제 리로드
///
/// ⚠ 실제 데이터 변환(Row->Data)과 데이터 검증은 파생 클래스가 담당
/// </summary>
public abstract class TSVDatabase<TData, TRow> : IDatabase<TData> where TData : class where TRow : class, ITsvRowWithId
{
    protected readonly string tableName;
    protected readonly bool logOnLoad;
    
    protected readonly StringComparer idComparer;

    protected readonly Dictionary<string, TData> byId;
    protected bool isLoaded;
    protected Task loadTask;

    public bool IsLoaded => isLoaded;
    public Task LoadTask => loadTask;

    protected TSVDatabase(string tableName, bool logOnLoad, StringComparer idComparer = null)
    {
        this.tableName = string.IsNullOrEmpty(tableName) ? "Table" : tableName;
        this.logOnLoad = logOnLoad;
        this.idComparer = idComparer ?? StringComparer.OrdinalIgnoreCase;

        byId = new Dictionary<string, TData>(this.idComparer);
        isLoaded = false;
        loadTask = null;
    }

    public Task EnsureLoadedAsync()
    {
        if (isLoaded)
            return Task.CompletedTask;

        if (loadTask != null)
            return loadTask;

        loadTask = LoadInternalAsync();
        return loadTask;
    }

    
    public bool TryGet(string id, out TData data)
    {
        if (string.IsNullOrEmpty(id))
        {
            data = null;
            return false;
        }

        return byId.TryGetValue(id, out data);
    }

    public TData GetOrNull(string id)
    {
        if (TryGet(id, out TData data))
            return data;

        return null;
    }

    public Task ReloadAsync()
    {
        loadTask = LoadInternalAsync();
        return loadTask;
    }
    
    // =======================
    // Core Load PipeLine
    // =======================
    protected async Task LoadInternalAsync()
    {
        byId.Clear();
        isLoaded = false;

        // 1) TSV Row 읽기
        List<TRow> rows = await TSVReader.ReadTableAsync<TRow>(tableName);
        if (rows == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Failed to load table '{tableName}'. rows=null");
            isLoaded = false;
            return;
        }

        // 2) Row -> Data 변환 + 등록
        int loaded = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            TRow row = rows[i];
            if (row == null)
                continue;

            if (string.IsNullOrEmpty(row.Id))
            {
                Debug.LogWarning($"[{GetType().Name}] Row {i}: missing Id. skipped.");
                continue;
            }

            TData data = ConvertRowToData(row, i);
            if (data == null)
                continue;

            // 파생 클래스에서 검증/클램프 등 처리
            ValidateData(data, row, i);

            string key = GetDataId(data, row);

            if (byId.ContainsKey(key))
            {
                Debug.LogWarning($"[{GetType().Name}] Duplicated Id='{key}'. Overwriting previous.");
                byId[key] = data;
            }
            else
            {
                byId.Add(key, data);
            }

            loaded++;
        }

        isLoaded = true;

        if (logOnLoad)
            Debug.Log($"[{GetType().Name}] Loaded {loaded} entries from table '{tableName}'.");
    }
    
    protected abstract TData ConvertRowToData(TRow row, int rowIndex);
    protected virtual string GetDataId(TData data, TRow row) => row.Id;

    protected virtual void ValidateData(TData data, TRow row, int rowIndex)
    {
    }
}