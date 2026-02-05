using Michsky.MUIP;
using UnityEngine;

public class HPProgressUIBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProgressBar hpBar;
    [SerializeField] private Player player;
    
    [Header("ProgressBar Options")]
    [SerializeField] private bool setInstant = true;
    [SerializeField] private float smoothPercentPerSec = 120f;
    [SerializeField] private bool forceOverrideEveryFrame = true;  

    private ProgressViewModel<int> hpVM;
    private bool isInitialized = false;
    
    private float targetPercent;     // 0~100
    private float displayedPercent;

    public void SetHPBar(ProgressBar hpBar)
    {
        this.hpBar = hpBar;
        TryInitialize();
    }

    private void Awake()
    {
        if (player == null)
            player = Player.Instance;

        if (hpBar == null)
            hpBar = GetComponentInChildren<ProgressBar>(true);
    }

    private void OnEnable()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (isInitialized)
            return;
        
        // 1. 플레이어 참조 확인 (DDOL 대응)
        if (player == null)
            player = Player.Instance;

        // 2. 두 참조가 모두 있어야 초기화 가능
        if (player == null || hpBar == null)
            return;
        
        LockProgressBarAsRenderer();

        if (hpVM != null)
            hpVM.Dispose();

        hpVM = new ProgressViewModel<int>(
            player.Stats.Resources.CurHp,
            player.Stats.MaxHpObs,
            v => v
        );
        
        hpVM.OnRatioChanged += OnHpRatioChanged;
        
        targetPercent = Mathf.Clamp(hpVM.Ratio * 100f, 0f, 100f);
        displayedPercent = targetPercent;
        hpBar.currentPercent = displayedPercent;
        hpBar.UpdateUI(); // Modern UI Pack 강제 갱신
        
        isInitialized = true;
        Debug.Log("[HPProgressUIBinder] ::: HP 바 바인딩 성공");
    }

    private void OnDisable()
    {
        if (hpVM != null)
        {
            hpVM.OnRatioChanged -= OnHpRatioChanged;
            hpVM.Dispose();
            hpVM = null;
        }

        isInitialized = false;
    }
    
    private void LockProgressBarAsRenderer()
    {
        hpBar.isOn = true;
        hpBar.restart = false;
        hpBar.invert = false;
        
        hpBar.speed = 0;
        hpBar.currentPercent = Mathf.Clamp(hpBar.currentPercent, 0f, 100f);
    }

    private void OnHpRatioChanged(float ratio)
    {
        targetPercent = Mathf.Clamp(ratio * 100f, 0f, 100f);

        if (setInstant)
        {
            displayedPercent = targetPercent;
            hpBar.currentPercent = displayedPercent;
        }
    }

    private void Update()
    {
        if (hpBar == null)
            return;

        if (setInstant == false)
        {
            float step = Mathf.Max(0f, smoothPercentPerSec) * Time.unscaledDeltaTime;
            displayedPercent = Mathf.MoveTowards(displayedPercent, targetPercent, step);
        }
        
        if (forceOverrideEveryFrame)
        {
            hpBar.currentPercent = displayedPercent;
        }
        else
        {
            if (Mathf.Abs(hpBar.currentPercent - displayedPercent) > 0.01f)
                hpBar.currentPercent = displayedPercent;
        }
    }
}
