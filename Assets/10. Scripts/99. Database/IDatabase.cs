using System.Threading.Tasks;

public interface IDatabase<TData> where TData : class
{
    Task EnsureLoadedAsync();
    
    bool IsLoaded { get; }
    
    Task LoadTask { get; }

    bool TryGet(string id, out TData data);
    
    TData GetOrNull(string id);

    Task ReloadAsync();
}