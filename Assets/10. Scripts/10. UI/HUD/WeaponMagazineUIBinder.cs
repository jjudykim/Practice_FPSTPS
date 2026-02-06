using UnityEngine;
using jjudy;

public class WeaponMagazineUIBinder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private MagazineUI magazineUI;

    [Header("Runtime (ReadOnly)")]
    [SerializeField] private WeaponBase boundWeapon;

    public void SetMagazineUI(MagazineUI magazineUI)
    {
        this.magazineUI = magazineUI;
    }

    private void Awake()
    {
        if (magazineUI == null)
            magazineUI = GetComponentInChildren<MagazineUI>();

        SetUIActive(false);
    }
    
    public void Bind(WeaponBase newWeapon)
    {
        Unbind();

        boundWeapon = newWeapon;

        if (boundWeapon == null || magazineUI == null)
        {
            SetUIActive(false);
            return;
        }
        
        SetUIActive(true);
        
        int maxAmmo = boundWeapon.Data != null ? boundWeapon.Data.MagazineSize : 0;
        magazineUI.Build(maxAmmo);

        int cur = boundWeapon.CurrentAmmo;
        magazineUI.SetFill(cur);
        magazineUI.SetEmptyVisual(cur == 0);
        
        boundWeapon.Ammo.OnValueChanged += OnAmmoChanged;
    }

    public void Unbind()
    {
        if (boundWeapon != null)
        {
            boundWeapon.Ammo.OnValueChanged -= OnAmmoChanged;
            boundWeapon = null;
        }
        
        SetUIActive(false);
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void OnAmmoChanged(int prev, int current)
    {
        if (magazineUI == null)
            return;

        magazineUI.SetFill(current);
        magazineUI.SetEmptyVisual(current == 0);
    }
    
    private void SetUIActive(bool active)
    {
        if (magazineUI == null)
            return;
        
        if (magazineUI.gameObject.activeSelf == active)
            return;

        magazineUI.gameObject.SetActive(active);
    }
}