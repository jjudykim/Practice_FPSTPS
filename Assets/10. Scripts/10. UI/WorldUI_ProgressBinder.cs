using UnityEngine;
using UnityEngine.UI;

public class WorldUI_ProgressBinder : MonoBehaviour
{
    [SerializeField] private Image staminaFill;

    private ProgressViewModel<int> staminaVM;

    private void Start()
    {
        var player = Player.Instance;

        staminaVM = new ProgressViewModel<int>(player.Stats.Resources.CurStamina
                                             , player.Stats.MaxStaminaObs
                                             , v => v);

        staminaVM.OnRatioChanged += ratio => staminaFill.fillAmount = ratio;
    }

    private void OnDestroy()
    {
        staminaVM.Dispose();
    }
}