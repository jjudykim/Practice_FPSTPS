using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class CrosshairUI : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private RectTransform up;
    [SerializeField] private RectTransform down;
    [SerializeField] private RectTransform left;
    [SerializeField] private RectTransform right;

    [Header("Visual")]
    [SerializeField] private float barLength = 12f;   
    [SerializeField] private float barThickness = 2f;

    [Header("Radius")]
    [SerializeField] private float minRadiusPx = 6f;   
    [SerializeField] private float maxRadiusPx = 60f;
    [SerializeField] private float radiusLerpSpeed = 15f;

    [Header("Spread -> Radius Mapping")]
    [SerializeField] private float spreadToPixels = 40f;   

    [Header("Recoil Kick (Draft)")]
    [SerializeField] private float kickAddPx = 10f;
    [SerializeField] private float kickDecaySpeed = 18f;
    
    private float targetRadius;
    private float currentRadius;

    private float recoilKick;
    
    private WeaponData weaponData;
    private bool isADS;

    private void Reset()
    {
        ApplyBarSize();
    }

    private void Awake()
    {
        ApplyBarSize();
        currentRadius = minRadiusPx;
        targetRadius = minRadiusPx;
    }

    private void Update()
    {
        // 1) recoilKick 자연 감소
        recoilKick = Mathf.MoveTowards(recoilKick, 0f, kickDecaySpeed);

        // 2) 목표 반경 계산
        float spreadAngle = GetCurrentSpreadAngleDeg();
        float spreadPx = spreadAngle * spreadToPixels;

        float raw = spreadPx + recoilKick;
        
        targetRadius = Mathf.Clamp(raw, minRadiusPx, maxRadiusPx);

        currentRadius = Mathf.Lerp(currentRadius, targetRadius, 1f - Mathf.Exp(-radiusLerpSpeed * Time.deltaTime));

        ApplyRadius(currentRadius);
    }

    public void SetContext(WeaponData data, bool ads)
    {
        weaponData = data;
        isADS = ads;
    }

    public void OnFired()
    {
        recoilKick += kickAddPx;
    }

    private float GetCurrentSpreadAngleDeg()
    {
        if (weaponData == null)
            return 0.2f;

        float adsSpread = weaponData.ADS_Spread;
        if (isADS)
            return adsSpread;

        return Mathf.Max(adsSpread * 3f, adsSpread);
    }

    private void ApplyRadius(float r)
    {
        if (up != null)
            up.anchoredPosition = new Vector2(0f, +r);
        if (down != null)
            down.anchoredPosition = new Vector2(0f, -r);
        if (left != null)
            left.anchoredPosition = new Vector2(-r, 0f);
        if (right != null)
            right.anchoredPosition = new Vector2(+r, 0f);
    }

    private void ApplyBarSize()
    {
        if (up != null)
            up.sizeDelta = new Vector2(barThickness, barLength);
        if (down != null)
            down.sizeDelta = new Vector2(barThickness, barLength);
        if (left != null)
            left.sizeDelta = new Vector2(barLength, barThickness);
        if (right != null)
            right.sizeDelta = new Vector2(barLength, barThickness);
    }
}
