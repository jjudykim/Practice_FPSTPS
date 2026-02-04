using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float height = 30f;
    [SerializeField] private float orthoSize = 20f;

    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Rotation Settings")]
    [SerializeField] private bool lockRotation = true;
    [SerializeField] private Vector3 lockedEuler = new Vector3(90f, 0f, 0f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;
        
        Vector3 pos = target.position;
        pos.y += height;
        transform.position = pos;
        
        if (lockRotation)
            transform.rotation = Quaternion.Euler(lockedEuler);
    }
    
    public void SetTarget(Transform t) => target = t;
}