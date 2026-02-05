using System;
using Unity.VisualScripting;
using UnityEngine;

public class AimProviderRouter : MonoBehaviour, IAimProvider
{
    [Header("Ref")] 
    [SerializeField] private CameraController cameraController;
    
    [Header("Providers")]
    [SerializeField] private MonoBehaviour firstPersonProviderBehaviour;
    [SerializeField] private MonoBehaviour quarterViewProviderBehaviour;
    
    private IAimProvider firstPersonProvider;
    private IAimProvider quarterViewProvider;

    private Transform fallbackMuzzle;

    public Camera AimCamera {
        get
        {
            var p = CurrentProvider;
            return p != null ? p.AimCamera : Camera.main;
        }
    }

    public Transform Muzzle
    {
        get
        {
            Transform m = (CurrentProvider != null) ? CurrentProvider.Muzzle : null;
            return m != null ? m : fallbackMuzzle;
        }
    }

    private IAimProvider CurrentProvider
    {
        get
        {
            if (cameraController == null)
                return firstPersonProvider != null ? firstPersonProvider : quarterViewProvider;

            return (cameraController.Mode == CameraController.CameraMode.FirstPerson)
                    ? firstPersonProvider
                    : quarterViewProvider;
        }
    }

    private void Reset()
    {
        AutoBind();
    }

    private void Awake()
    {
        AutoBind();
    }

    private void OnEnable()
    {
        AutoBind();
    }
    
    private void AutoBind()
    {
        if (firstPersonProviderBehaviour == null)
        {
            firstPersonProviderBehaviour = FindProviderByHint("First");
        }

        if (quarterViewProviderBehaviour == null)
        {
            quarterViewProviderBehaviour = FindProviderByHint("Quarter");
        }

        firstPersonProvider = firstPersonProviderBehaviour as IAimProvider;
        quarterViewProvider = quarterViewProviderBehaviour as IAimProvider;
        
        if (cameraController == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
                cameraController = mainCam.GetComponent<CameraController>();
        }
    }
    
    private MonoBehaviour FindProviderByHint(string nameHint)
    {
        var list = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < list.Length; i++)
        {
            var mb = list[i];
            if (mb == null) 
                continue;
            
            if (mb is IAimProvider == false)
                continue;
            
            if (mb.GetType().Name.Contains(nameHint))
                return mb;
        }

        return null;
    }

    public void SetMuzzle(Transform newMuzzle)
    {
        fallbackMuzzle = newMuzzle;
        
        if (quarterViewProviderBehaviour is QuarterViewAimProvider q)
            q.SetMuzzle(newMuzzle);
        
        if (firstPersonProviderBehaviour is FirstPersonAimProvider f)
            f.SetMuzzle(newMuzzle);
    }

    public Ray GetAimRay()
    {
        var p = CurrentProvider;
        if (p == null)
            return new Ray(Vector3.zero, Vector3.forward);

        return p.GetAimRay();
    }

    public bool TryGetAimPoint(float maxDistance, LayerMask mask, out Vector3 hitPoint)
    {
        var p = CurrentProvider;
        if (p == null)
        {
            hitPoint = default;
            return false;
        }

        return CurrentProvider.TryGetAimPoint(maxDistance, mask, out hitPoint);
    }
}