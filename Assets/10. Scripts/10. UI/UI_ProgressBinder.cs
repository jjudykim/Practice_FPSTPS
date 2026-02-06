using UnityEngine;
using UnityEngine.UI;

public class UI_ProgressBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatController combat;
    [SerializeField] private Slider ReloadUI;
    
    [Header("Images")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image reloadFill;

    private ProgressViewModel<int> staminaVM;
    private ProgressViewModel<float> ReloadFillVM;
    
    private void Start()
    {
        var player = Player.Instance;

        if (combat == null && player != null)
            combat = player.GetComponent<PlayerCombatController>();

        if (player == null || combat == null)
            return;
        
        staminaVM = new ProgressViewModel<int>(player.Stats.Resources.CurStamina
                                              , player.Stats.MaxStaminaObs
                                              , v => v);
        ReloadFillVM = new ProgressViewModel<float>(combat.ReloadElapsedObs
                                                    , combat.ReloadDurationObs
                                                    , v => v);

        BindEvents();
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        DisposeViewModels();
    }

    private void BindEvents()
    {
        if (combat == null)
            return;

        if (combat == null || staminaVM == null || ReloadFillVM == null)
            return;

        UnbindEvents();

        staminaVM.OnRatioChanged += HandleStaminaRatioChanged;
        ReloadFillVM.OnRatioChanged += HandleReloadRatioChanged;
        combat.ReloadVisibleObs.OnValueChanged += HandleReloadVisibleChanged;

        HandleReloadVisibleChanged(false, combat.ReloadVisibleObs.Value);
    }

    private void UnbindEvents()
    {
        if (staminaVM != null)
            staminaVM.OnRatioChanged -= HandleStaminaRatioChanged;
        if (ReloadFillVM != null)
            ReloadFillVM.OnRatioChanged -= HandleReloadRatioChanged;

        if (combat != null)
            combat.ReloadVisibleObs.OnValueChanged -= HandleReloadVisibleChanged;
    }

    private void DisposeViewModels()
    {
        staminaVM?.Dispose();
        staminaVM = null;
        ReloadFillVM?.Dispose();
        ReloadFillVM = null;
    }
    
    private void HandleStaminaRatioChanged(float ratio)
    {
        if (staminaFill != null) 
            staminaFill.fillAmount = ratio;
    }

    private void HandleReloadRatioChanged(float ratio)
    {
         if (ReloadUI != null) 
             ReloadUI.value = ratio;
    }

    private void HandleReloadVisibleChanged(bool prev, bool cur)
    {
        if (ReloadUI != null)
        {
            ReloadUI.gameObject.SetActive(cur);
        }
    }
}