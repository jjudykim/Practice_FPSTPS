using System;
using Michsky.MUIP;
using UnityEngine;

public class InGameUIIntializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatController player;
    [SerializeField] private GameObject playerUIBinder;
    
    [Header("Global HUD")]
    [SerializeField] private MagazineUI magazineUI;
    [SerializeField] private ProgressBar hpBar;
    [SerializeField] private PlayerUIModeSwitcher modeSwitcher;
    [SerializeField] private CrosshairUI crosshairUI;

    private void Awake()
    {
        var playerObj = Player.Instance;
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerCombatController>();
            if (player != null) playerUIBinder = player.UIBinder;
        }
    }

    private void Start()
    {
        ApplyBinding();
    }

    private void OnEnable()
    {
        ApplyBinding();
    }

    private void ApplyBinding()
    {
        if (Player.Instance == null) 
            return;
   
        if (player == null)
            player = Player.Instance.GetComponent<PlayerCombatController>();
   
        if (player != null)
        {
            playerUIBinder = player.UIBinder;
            
            if (modeSwitcher != null) modeSwitcher.SetWorldUIRoot(player.WorldUIRoot);
            player.SetCrossHairUI(crosshairUI);
   
            if (playerUIBinder != null)
            {
                var magazineUIBinder = playerUIBinder.GetComponent<WeaponMagazineUIBinder>();
                if (magazineUIBinder != null) magazineUIBinder.SetMagazineUI(magazineUI);
   
                var hpUIBinder = playerUIBinder.GetComponent<HPProgressUIBinder>();
                if (hpUIBinder != null) hpUIBinder.SetHPBar(hpBar);
            }
   
            Debug.Log("[InGameUIIntializer] ::: 모든 UI 바인딩 완료");
        }  
    } 
}
