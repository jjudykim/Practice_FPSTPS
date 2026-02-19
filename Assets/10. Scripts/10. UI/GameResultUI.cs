using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject overPanel;
    
    [SerializeField] private Button clearExitButton;
    [SerializeField] private Button overExitButton;

    private bool isTransitioning = false;
    
    private void Start()
    {
        Managers.Instance.Game.OnStateChanged -= HandleStateChanged;
        Managers.Instance.Game.OnStateChanged += HandleStateChanged;
        
        clearPanel.SetActive(false);
        overPanel.SetActive(false);
        
        if (clearExitButton != null)
            clearExitButton.onClick.AddListener(OnExitToTown);
        if (overExitButton != null)
            overExitButton.onClick.AddListener(OnExitToTown);
    }

    private void OnDestroy()
    {
        if (Managers.Instance != null && Managers.Instance.Game != null)
        {
            Managers.Instance.Game.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.GameClear)
            clearPanel.SetActive(true);
        
        if (state == GameState.GameOver)
            overPanel.SetActive(true);

        if (state == GameState.GameClear || state == GameState.GameOver)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnExitToTown()
    {
        if (isTransitioning)
            return;

        StartCoroutine(CoExitToTown());
    }

    private IEnumerator CoExitToTown()
    {
        isTransitioning = true;

        yield return StartCoroutine(Managers.Instance.Scene.CoFadeOut(0.5f));
        
        if (Managers.Instance.SaveData != null)
        {
            Managers.Instance.SaveData.Save();
            Debug.Log($"[GameResult] 세이브 데이터 저장 완료 (Slot: {Managers.Instance.SaveData.CurrentSlotIndex})");
        }
        
        Managers.Instance.Game.ResetGameSession();
        Managers.Instance.Game.ResetToDefault();

        if (Player.Instance != null)
        {
            var receiver = Player.Instance.GetComponentInChildren<PlayerDamageReceiver>();
            if (receiver != null)
                receiver.Resurrect();
            
            Player.Instance.ResetForTown();
        }

        clearPanel.SetActive(false);
        overPanel.SetActive(false);

        Managers.Instance.Scene.LoadScene("TownScene");

        isTransitioning = false;
    }
}