using UnityEngine;

public class FirstPersonAimProvider : MonoBehaviour, IAimProvider
{
    [Header("Ref")] 
    [SerializeField] private Camera aimCamera;
    [SerializeField] public Transform muzzle;
    
    public Camera AimCamera => aimCamera;
    public Transform Muzzle => muzzle;

    private void Reset()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    public Ray GetAimRay()
    {
        // 카메라가 없을 경우를 대비 : 정면으로 레이
        if (aimCamera == null)
            return new Ray(transform.position, transform.forward);

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        return aimCamera.ScreenPointToRay(screenCenter);
    }

    public bool TryGetAimPoint(float maxDistance, LayerMask mask, out Vector3 hitPoint)
    {
        Ray ray = GetAimRay();

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            return true;
        }

        hitPoint = ray.origin + ray.direction * maxDistance;
        return false;
    }
}