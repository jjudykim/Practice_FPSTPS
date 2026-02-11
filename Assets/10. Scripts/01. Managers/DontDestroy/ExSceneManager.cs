using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExSceneManager
{
    private FadeUI fadeUI;

    public IEnumerator CoFadeOut(float duration) => fadeUI.CoFade(1f, duration);
    public IEnumerator CoFadeIn(float duration) => fadeUI.CoFade(0f, duration);

    public void Init()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/UI/FadeCanvas");
        if (prefab == null)
        {
            Debug.LogError("FadeCanvas 프리팹을 찾을 수 없습니다! Resources/Prefabs/UI/FadeCanvas 경로를 확인하세요.");
            return;
        }
        
        var go = Object.Instantiate(prefab, Managers.Instance.transform);
        fadeUI = go.GetComponent<FadeUI>();
        
        fadeUI.SetAlpha(0f);
    }
    
    public void LoadScene(string name)
    {
        Managers.Instance.StartCoroutine(CoTransition(name));
    }
    
    private IEnumerator CoTransition(string name)
    {
        // 1. Fade Out
        yield return Managers.Instance.StartCoroutine(fadeUI.CoFade(1f, 0.5f));

        if (name == "TownScene" || name == "LobbyScene")
        {
            Managers.Instance.Game.ResetToDefault();
        }

        // 2. Scene Load (비동기)
        AsyncOperation op = SceneManager.LoadSceneAsync(name);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);

        yield return Managers.Instance.StartCoroutine(fadeUI.CoFade(0f, 0.5f));
    }
}
