using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagazineUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform magazineRect;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    
    [Header("Background Image Swap")]
    [SerializeField] private Image magazineBackground;  
    [SerializeField] private Sprite normalSprite;   
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private bool swapBackgroundOnEmpty = true;

    [Header("Template (Child Bullet)")]
    [SerializeField] private Image bulletTemplate;
    [SerializeField] private bool templateShouldBeHidden = true;

    [Header("Auto Resize")]
    [SerializeField] private bool autoResizeMagazine = true;

    // Runtime
    private readonly List<Image> bullets = new List<Image>();
    private LayoutElement magazineLayoutElement;

    private int maxAmmo = 0;

    private void Awake()
    {
        if (magazineRect == null)
            magazineRect = GetComponent<RectTransform>();

        if (layoutGroup == null)
            layoutGroup = GetComponent<VerticalLayoutGroup>();
        
        if (magazineBackground == null)
            magazineBackground = GetComponent<Image>();

        if (bulletTemplate == null)
        {
            Debug.LogError("[UIMagazineBullets] Bullet Template is null.");
            return;
        }
        
        magazineLayoutElement = GetComponent<LayoutElement>();
        if (magazineLayoutElement == null)
            magazineLayoutElement = gameObject.AddComponent<LayoutElement>();
    }
    
    
    public void Build(int newMaxAmmo)
    {
        if (bulletTemplate == null)
            return;

        newMaxAmmo = Mathf.Max(0, newMaxAmmo);
        
        if (maxAmmo == newMaxAmmo && bullets.Count == newMaxAmmo)
            return;

        maxAmmo = newMaxAmmo;
        
        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i] != null)
                Destroy(bullets[i].gameObject);
        }
        bullets.Clear();
        
        Transform parent = this.transform;

        // maxAmmo 만큼 Bullet 생성
        for (int i = 0; i < maxAmmo; i++)
        {
            Image b = Instantiate(bulletTemplate, parent);
            b.gameObject.name = $"Bullet_{i:D2}";
            b.gameObject.SetActive(true);
            bullets.Add(b);
        }

        // 탄창 RectTransform 크기 조절
        if (autoResizeMagazine)
            ResizeMagazineToFit(maxAmmo);
    }
    
    public void SetFill(int currentAmmo)
    {
        if (maxAmmo <= 0)
            return;

        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i] == null) 
                continue;
            
            bullets[i].gameObject.SetActive(i < currentAmmo);
        }
    }
    
    public void SetEmptyVisual(bool isEmpty)
    {
        if (swapBackgroundOnEmpty == false)
            return;

        if (magazineBackground == null)
            return;
        
        if (isEmpty)
        {
            if (emptySprite != null)
                magazineBackground.sprite = emptySprite;
        }
        else
        {
            if (normalSprite != null)
                magazineBackground.sprite = normalSprite;
        }
    }
    
    private void ResizeMagazineToFit(int count)
    {
        if (layoutGroup == null || bulletTemplate == null || magazineLayoutElement == null)
            return;
        
        float paddingVertical = layoutGroup.padding.top + layoutGroup.padding.bottom;
        
        float bulletH = GetTemplatePreferredHeight();
        float spacing = layoutGroup.spacing;
        
        float totalH;
        if (count <= 0)
        {
            totalH = paddingVertical;
        }
        else
        {
            totalH = paddingVertical
                     + (bulletH * count)
                     + (spacing * Mathf.Max(0, count - 1));
        }
        
        magazineRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalH);
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(magazineRect);
    }
    
    private float GetTemplatePreferredHeight()
    {
        RectTransform tr = bulletTemplate.rectTransform;
        
        float preferred = LayoutUtility.GetPreferredHeight(tr);
        if (preferred > 0.01f)
            return preferred;
        
        float rectH = tr.rect.height;
        if (rectH > 0.01f)
            return rectH;
        
        float sd = tr.sizeDelta.y;
        if (sd > 0.01f)
            return sd;
        
        return 20f;
    }
}
