using UnityEngine;

/// <summary>
/// Crosshair UI (Two-State Lerp)
/// - Aim(ADS) OFF: hipRadiusPx 로 벌어진 상태
/// - Aim(ADS) ON : adsRadiusPx 로 모인 상태
/// - Aim 상태가 바뀔 때에만 목표 반경이 바뀌며, 그 사이 보간만 일어남
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    [Header("Parts (RectTransforms)")]
    [SerializeField] private RectTransform up;
    [SerializeField] private RectTransform down;
    [SerializeField] private RectTransform left;
    [SerializeField] private RectTransform right;

    [Header("Follow Mode (QuarterView)")]
    [SerializeField] private bool followMouse = false;
    [SerializeField] private Canvas parentCanvas;

    [Header("Two-State Radius (Pixels)")]
    [SerializeField] private float hipRadiusPx = 28f;
    [SerializeField] private float adsRadiusPx = 10f;

    [Header("Lerp")]
    [SerializeField] private float radiusLerpSpeed = 18f;

    [Header("Clamp (Safety)")]
    [SerializeField] private float minRadiusPx = 2f;
    [SerializeField] private float maxRadiusPx = 80f;

    // 내부 상태
    private RectTransform selfRect;
    private RectTransform parentRect;

    private WeaponData weaponData; // 필요하면 유지(현재는 반경 계산에 사용하지 않음)
    private bool isADS;

    private float currentRadius;
    private float targetRadius;

    private void Awake()
    {
        selfRect = transform as RectTransform;
        parentRect = transform.parent as RectTransform;

        // 초기 상태: hip 기준으로 시작
        targetRadius = Mathf.Clamp(hipRadiusPx, minRadiusPx, maxRadiusPx);
        currentRadius = targetRadius;

        ApplyRadius(currentRadius);
        UpdateRootPosition(force: true);
    }

    private void Update()
    {
        // 1) 루트 위치 갱신 (쿼터뷰: 마우스 추적 / 1인칭: 중앙 고정)
        UpdateRootPosition();

        // 2) 목표 반경은 "두 상태" 중 하나만 선택
        float desired = isADS ? adsRadiusPx : hipRadiusPx;
        targetRadius = Mathf.Clamp(desired, minRadiusPx, maxRadiusPx);

        // 3) 목표로 보간 (Aim 전환 시에만 target이 바뀌므로, 그때만 움직임)
        float t = 1f - Mathf.Exp(-radiusLerpSpeed * Time.deltaTime);
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, t);

        ApplyRadius(currentRadius);
    }

    /// <summary>
    /// QuarterView(마우스 추적) / FirstPerson(중앙 고정) 전환용
    /// PlayerCombatController의 RefreshCrosshairVisibility에서 호출하던 흐름 유지.
    /// </summary>
    public void SetFollowMouse(bool enabled)
    {
        followMouse = enabled;
        UpdateRootPosition(force: true);
    }

    /// <summary>
    /// 무기 데이터/ADS 상태를 외부에서 주입받습니다.
    /// - weaponData는 현재 반경 계산에 사용하지 않지만,
    ///   향후 색상/가시성/기타 표현에 쓸 수 있어 유지합니다.
    /// </summary>
    public void SetContext(WeaponData data, bool ads)
    {
        weaponData = data;

        // ADS 상태가 바뀌었을 때만 반경 목표가 바뀌므로
        // 이 함수가 호출될 때 isADS가 변경되면 "보간 전환"이 발생합니다.
        isADS = ads;
    }

    private void UpdateRootPosition(bool force = false)
    {
        if (selfRect == null || parentRect == null)
            return;

        if (followMouse)
        {
            Vector2 screenPos = Input.mousePosition;

            Camera uiCam = null;
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCam = parentCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCam, out Vector2 localPoint))
                selfRect.anchoredPosition = localPoint;
        }
        else
        {
            // 중앙 고정
            selfRect.anchoredPosition = Vector2.zero;
        }
    }

    private void ApplyRadius(float r)
    {
        if (up != null) up.anchoredPosition = new Vector2(0f, +r);
        if (down != null) down.anchoredPosition = new Vector2(0f, -r);
        if (left != null) left.anchoredPosition = new Vector2(-r, 0f);
        if (right != null) right.anchoredPosition = new Vector2(+r, 0f);
    }

#if UNITY_EDITOR
    // 에디터에서 값 조정할 때 즉시 반영되도록
    private void OnValidate()
    {
        float hip = Mathf.Clamp(hipRadiusPx, minRadiusPx, maxRadiusPx);
        float ads = Mathf.Clamp(adsRadiusPx, minRadiusPx, maxRadiusPx);

        hipRadiusPx = hip;
        adsRadiusPx = ads;

        if (Application.isPlaying == false)
        {
            // 플레이 중이 아니면 그냥 hip 기준으로 미리보기
            ApplyRadius(hipRadiusPx);
        }
    }
#endif
}
