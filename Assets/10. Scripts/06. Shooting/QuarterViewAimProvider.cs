using UnityEngine;

public class QuarterViewAimProvider : MonoBehaviour, IAimProvider
{
    [Header("Ref")]
    [SerializeField] private Camera aimCamera;

    [SerializeField] private Transform muzzle;
    
    public Camera AimCamera => aimCamera;
    public Transform Muzzle => muzzle;

    private void Reset()
    {
        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    public Ray GetAimRay()
    {
        if (aimCamera == null)
            return new Ray(transform.position, transform.forward);

        Vector3 mouse = Input.mousePosition;
        return aimCamera.ScreenPointToRay(mouse);
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