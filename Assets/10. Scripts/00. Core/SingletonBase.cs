using System;
using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    private static readonly object lockObj = new object();
    private static bool applicationIsQuitting = false;
    private static bool isInitialized = false;

    public static bool IsInitialized => isInitialized && instance != null;
    protected virtual bool AllowAutoCreate => false;
    
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] ::: {typeof(T).Name} 접근 시도됨: 애플리케이션 종료 중");
                return null;
            }

            lock (lockObj)
            {
                if (instance != null)
                    return instance;

                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    Debug.LogWarning($"[Singleton] ::: {typeof(T).Name} not found.");
                    return null;
                }
                
                var sb = instance as SingletonBase<T>;
                if (sb != null && !isInitialized)
                {
                    sb.InternalInitialize();
                }

                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[Singleton] ::: 중복 인스턴스 감지 : {gameObject.name} 파괴");
            Destroy(gameObject);
            return;
        }

        instance = this as T;
        
        DontDestroyOnLoad(transform.root.gameObject);

        if (!isInitialized)
            InternalInitialize();
    }

    private void InternalInitialize()
    {
        // 중복 호출 방지
        if (isInitialized)
            return;

        OnInitialize();
        isInitialized = true;
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            OnDispose();
            instance = null;
            isInitialized = false;
        }
    }
    
    protected virtual void OnInitialize() { }
    protected virtual void OnDispose() { }

    public static void ResetInstance()
    {
        if (instance == null)
            return;

        var sb = instance as SingletonBase<T>;
        sb?.OnDispose();

#if UNITY_EDITOR
        DestroyImmediate((instance as MonoBehaviour)?.gameObject);
#else
        Destroy((instance as MonoBehaviour)?.gameObject);
#endif

        instance = null;
        isInitialized = false;
    }
}
