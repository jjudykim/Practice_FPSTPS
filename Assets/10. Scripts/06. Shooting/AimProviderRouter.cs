using Unity.VisualScripting;
using UnityEngine;

public class AimProviderRouter : MonoBehaviour, IAimProvider
{
    [Header("Ref")] 
    [SerializeField] private CameraController cameraController;
    
    [Header("Providers")]
    [SerializeField] private MonoBehaviour firstPersonProviderSource;
    [SerializeField] private MonoBehaviour quarterViewProviderSource;
    
    private IAimProvider firstPersonProvider;
    private IAimProvider quarterViewProvider;

    private Transform fallbackMuzzle;

    public Camera AimCamera => CurrentProvider.AimCamera;
    public Transform Muzzle
    {
        get
        {
            Transform m = CurrentProvider != null ? CurrentProvider.Muzzle : null;
            return m != null ? m : fallbackMuzzle;
        }
    }

    private IAimProvider CurrentProvider
    {
        get
        {
            if (cameraController == null)
                return firstPersonProvider;

            return (cameraController.Mode == CameraController.CameraMode.FirstPerson)
                    ? firstPersonProvider
                    : quarterViewProvider;
        }
    }

    private void Awake()
    {
        firstPersonProvider = firstPersonProviderSource as IAimProvider;
        quarterViewProvider = quarterViewProviderSource as IAimProvider;
    }
    
    public void SetMuzzle(Transform newMuzzle)
    {
        fallbackMuzzle = newMuzzle;
        
        if (quarterViewProviderSource is QuarterViewAimProvider q)
            q.SetMuzzle(newMuzzle);
    }

    public Ray GetAimRay()
    {
        return CurrentProvider.GetAimRay();
    }

    public bool TryGetAimPoint(float maxDistance, LayerMask mask, out Vector3 hitPoint)
    {
        return CurrentProvider.TryGetAimPoint(maxDistance, mask, out hitPoint);
    }
}