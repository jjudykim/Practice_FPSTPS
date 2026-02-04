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
    private float targetPercent;     // 0~100
    private float displayedPercent;

    private void Awake()
    {
        if (player == null)
            player = Player.Instance;

        if (hpBar == null)
            hpBar = GetComponentInChildren<ProgressBar>(true);
    }

    private void OnEnable()
    {
        if (player == null || hpBar == null)
        {
            Debug.LogWarning("[HUD_HPProgressBinder] Missing references. player or hpBar is null.");
            return;
        }
        LockProgressBarAsRenderer();
        
        hpVM = new ProgressViewModel<int>(
            player.Stats.Resources.CurHp,
            player.Stats.MaxHpObs,
            v => v
        );

        hpVM.OnRatioChanged += OnHpRatioChanged;

        // 초기 동기화
        targetPercent = Mathf.Clamp(hpVM.Ratio * 100f, 0f, 100f);
        displayedPercent = targetPercent;
        hpBar.currentPercent = displayedPercent;
    }
    
    private void OnDisable()
    {
        if (hpVM != null)
        {
            hpVM.OnRatioChanged -= OnHpRatioChanged;
            hpVM.Dispose();
            hpVM = null;
        }
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
