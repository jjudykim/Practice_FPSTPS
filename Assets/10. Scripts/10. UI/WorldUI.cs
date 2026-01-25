using Unity.IntegerTime;
using UnityEngine;

public class WorldUI : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;

    [Header("Billboard")] 
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private bool lockYRotationOnly = false;
    
    [SerializeField] private bool useLateUpdate = true;

    public void Init(Transform newTarget, Camera cam, Vector3 offset)
    {
        target = newTarget;

        if (cam != null)
            targetCamera = cam;

        worldOffset = offset;

        EnsureCamera();
        ForceUpdateNow();
    }
    
    private void Reset()
    {
        useLateUpdate = true;
        faceCamera = true;
        lockYRotationOnly = true;
        worldOffset = Vector3.zero;
    }

    private void Awake()
    {
        EnsureCamera();
    }

    private void OnEnable()
    {
        EnsureCamera();
        ForceUpdateNow();
    }

    private void Update()
    {
        if (useLateUpdate == false)
            Tick();
    }

    private void LateUpdate()
    {
        if (useLateUpdate)
            Tick();
    }

    private void Tick()
    {
        if (target != null)
        {
            transform.position = target.position + worldOffset;
        }

        if (faceCamera == false)
            return;

        EnsureCamera();
        if (targetCamera == null)
            return;

        if (lockYRotationOnly)
        {
            Vector3 camPos = targetCamera.transform.position;
            Vector3 dir = transform.position - camPos;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 lookDir = (camPos - transform.position);
                lookDir.y = 0f;
                
                if (lookDir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            }
        }
        else
        {
            Vector3 lookDir = transform.position - targetCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(lookDir, targetCamera.transform.up);
        }
    }

    private void EnsureCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }
    
    public void ForceUpdateNow()
    {
        Tick();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        ForceUpdateNow();
    }

    public void SetOffset(Vector3 newOffset)
    {
        worldOffset = newOffset;
        ForceUpdateNow();
    }
}
