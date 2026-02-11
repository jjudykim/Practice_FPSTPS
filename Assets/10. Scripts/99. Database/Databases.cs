using System;
using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class Databases : SingletonBase<Databases>
{
    public WeaponDatabase Weapon { get; private set; }
    public BulletDatabase Bullet { get; private set; }
    public ItemDatabase Item { get; private set; }
    
    protected override bool AllowAutoCreate => true;
    
    // 전체 DB 로딩 완료 여부
    public bool IsLoaded { get; private set; }
    
    private Task preloadTask;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Weapon = new WeaponDatabase();
        Bullet = new BulletDatabase();
        Item = new ItemDatabase();
        
        IsLoaded = false;
    }
    
    // GameManager.Awake() 또는 Game초기화 루틴에서 호출
    // 또는 **별도 BootScene에서 완료 후 다음 씬 진입하도록**
    public Task PreloadAllAsync()
    {
        if (IsLoaded)
            return Task.CompletedTask;

        if (preloadTask != null)
            return preloadTask;

        preloadTask = PreloadAllInternalAsync();
        return preloadTask;
    }
    
    private async Task PreloadAllInternalAsync()
    {
        try
        {
            if (Weapon != null)
                await Weapon.EnsureLoadedAsync();
            
            if (Bullet != null)
                await Bullet.EnsureLoadedAsync();

            if (Item != null)
                await Item.EnsureLoadedAsync();
            
            IsLoaded = true;
            Debug.Log("[Databases] PreloadAllAsync completed.");
        }
        catch (Exception e)
        {
            IsLoaded = false;
            Debug.LogError($"[Databases] PreloadAllAsync failed.\n{e}");
        }
    }
}