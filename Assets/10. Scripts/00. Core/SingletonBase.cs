using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;
    private static readonly object lockObj = new Object();
    private static bool applicationIsQuitting = false;
    private static bool isInitialized = false;

    public static bool IsInitialized => isInitialized && instance != null;

    [Obsolete("Obsolete")]
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] ::: {typeof(T)} 접근 시도됨: 애플리케이션 종료 중");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        var singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }

                    (instance as SingletonBase<T>)?.OnInitialize();
                    isInitialized = true;
                }

                return instance;
            }
        }
    }
    
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this);

            OnInitialize();
            isInitialized = true;
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[Singleton] ::: 중복 인스턴스 감지 : {gameObject.name} 파괴");
            Destroy(gameObject);
        }
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
            applicationIsQuitting = true;
        }
    }
    
    /// <summary>
    /// 싱글톤 초기화 시점에서 호출됨 (오버라이드 OK)
    /// </summary>
    protected virtual void OnInitialize() { }
    
    /// <summary>
    /// 싱글톤 제거 시점에서 호출됨 (오버라이드 OK)
    /// </summary>
    protected virtual void OnDispose() { }

    public static void ResetInstance()
    {
        if (instance != null)
        {
            (instance as SingletonBase<T>)?.OnDispose();
            
#if UNITY_EDITOR
            DestroyImmediate((instance as MonoBehaviour)?.gameObject);
#else
            Destroy((instance as MonoBehaviour)?.gameObject);
#endif  
            instance = null;
            isInitialized = false;
            applicationIsQuitting = false;
        }
    }
}