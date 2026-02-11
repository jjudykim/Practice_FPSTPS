using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimLineController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MonoBehaviour aimProviderBehaviour;
    [SerializeField] private Transform fallbackMuzzle;

    [Header("Weapon Range Source")]
    [SerializeField] private PlayerCombatController combatController;
    
    [Header("Aim Point Settings")]
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private float maxDistance = 200f;

    [Header("Visual")]
    [SerializeField] private bool showOnlyWhenAiming = true;
    [SerializeField] private bool hideIfNoProvider = true;
    

    [SerializeField] private Color hitColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color missColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private float width = 0.03f;

    [Header("Line Start Mode")] 
    [SerializeField] private bool startFromMuzzle = true;

    private LineRenderer lr;
    private IAimProvider aimProvider;
    private bool isAiming;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.enabled = false;

        aimProvider = aimProviderBehaviour as IAimProvider;

        if (aimProvider == null && hideIfNoProvider)
            Debug.LogWarning("[AimLineController] IAimProvider is null. Aim line will stay hidden.");
    }

    private void LateUpdate()
    {
        if (showOnlyWhenAiming && !isAiming)
        {
            if (lr.enabled) 
                lr.enabled = false;
            
            return;
        }

        if (aimProvider == null)
        {
            if (hideIfNoProvider && lr.enabled) 
                lr.enabled = false;
            return;
        }
        
        float runtimeMaxDistance = maxDistance;
        
        if (combatController != null)
            runtimeMaxDistance = combatController.GetCurrentWeaponAimMaxDistance(maxDistance);

        bool hasHit = aimProvider.TryGetAimPoint(runtimeMaxDistance, aimMask, out Vector3 aimPoint);
        Vector3 start;
        if (startFromMuzzle)
        {
            Transform muzzle = aimProvider.Muzzle != null ? aimProvider.Muzzle : fallbackMuzzle;
            if (muzzle == null)
            {
                if (lr.enabled) 
                    lr.enabled = false;
                
                return;
            }

            start = muzzle.position;
        }
        else
        {
            start = aimProvider.GetAimRay().origin;
        }

        // 선 그리기
        if (lr.enabled == false) lr.enabled = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, aimPoint);

        Color c = hasHit ? hitColor : missColor;
        lr.startColor = c;
        lr.endColor = c;
    }
    
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;

        if (lr != null)
            lr.enabled = false;
    }
}
