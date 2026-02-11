using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{ 
    [Header("UI Groups")]
    [SerializeField] private GameObject root;
   
    [Header("Buttons")]
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button townBtn;
    [SerializeField] private Button lobbyBtn;
    
    [Header("Scene Names")]
    [SerializeField] private string townSceneName = "TownScene";
    [SerializeField] private string lobbySceneName = "LobbyScene";
   
    private bool isPaused;
    public bool IsPaused => isPaused;

    private bool isTransitioning = false;
    
    private void Awake()
    {
        continueBtn.onClick.AddListener(GoToProcess);
        townBtn.onClick.AddListener(GoToTown);
        lobbyBtn.onClick.AddListener(GoToLobby);
    }
    
    // ==========================================
    //              Core Logic
    // ==========================================
    
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        root.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // ==========================================
    //             Button Actions
    // ==========================================

    private void GoToProcess()
    {
        Resume();
    }

    private void GoToTown()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        
        Time.timeScale = 1f;
        isPaused = false;
        root.SetActive(false);

        if (Player.Instance != null)
        {
            Player.Instance.ApplyDamage(999999);
        }
    }
    
    private void GoToLobby()
    {
        Time.timeScale = 1f;
        isPaused = false;
        root.SetActive(false);
        
        Managers.Instance.Scene.LoadScene(lobbySceneName);
    }
}
