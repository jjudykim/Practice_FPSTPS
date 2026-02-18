using System;
using Michsky.MUIP;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatController player;
    [SerializeField] private GameObject playerUIBinder;
    
    [Header("Global HUD")]
    [SerializeField] private MagazineUI magazineUI;
    [SerializeField] private ProgressBar hpBar;
    [SerializeField] private PlayerUIModeSwitcher modeSwitcher;
    [SerializeField] private CrosshairUI crosshairUI;

    [Header("Inventory UI")]
    [SerializeField] private InGameInventoryUI inventoryUI;
    
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
        InitializeInventory();
    }
    
    private void Update()
    {
        if (Managers.Instance != null && Managers.Instance.Input.Inventory)
        {
            ToggleInventory();
        }
    }
    
    private void InitializeInventory()
    {
        if (inventoryUI != null && Managers.Instance != null)
        {
            inventoryUI.Initialize(Managers.Instance.Game.Session.Inventory);
            inventoryUI.gameObject.SetActive(false);
        }
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null)
            return;
        
        bool isActive = !inventoryUI.gameObject.activeSelf;
        inventoryUI.gameObject.SetActive(isActive);

        if (isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Managers.Instance.Input.GamePlayInputEnable = false;
            inventoryUI.RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Managers.Instance.Input.GamePlayInputEnable = true;
        }
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
   
            Debug.Log("[InGameUIController] ::: 모든 UI 바인딩 완료");
        }  
    } 
}
