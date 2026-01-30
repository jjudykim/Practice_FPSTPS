using UnityEngine;
using UnityEngine.UI;

public class WorldUI_ProgressBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatController combat;
    [SerializeField] private Slider ReloadUI;
    
    [Header("Images")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image reloadFill;

    private ProgressViewModel<int> staminaVM;
    private ProgressViewModel<float> ReloadFillVM;
    private void Awake()
    {
        if (combat == null)
            combat = Player.Instance.gameObject.GetComponent<PlayerCombatController>();
    }

    private void Start()
    {
        var player = Player.Instance;

        staminaVM = new ProgressViewModel<int>(player.Stats.Resources.CurStamina
                                             , player.Stats.MaxStaminaObs
                                             , v => v);
        ReloadFillVM = new ProgressViewModel<float>(combat.ReloadElapsedObs
                                                    , combat.ReloadDurationObs
                                                    , v => v);

        staminaVM.OnRatioChanged += ratio => staminaFill.fillAmount = ratio;
        ReloadFillVM.OnRatioChanged += ratio => ReloadUI.value = ratio;

        combat.ReloadVisibleObs.OnValueChanged += (prev, cur) => { ReloadUI.gameObject.SetActive(cur); };
        
        ReloadUI.gameObject.SetActive(combat.ReloadVisibleObs.Value);
    }

    private void OnDestroy()
    {
        staminaVM.Dispose();
    }
}