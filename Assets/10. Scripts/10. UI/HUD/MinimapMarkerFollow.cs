using UnityEngine;

public class MinimapMarkerFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float yOffset = 2f;
    [SerializeField] private bool lockFlatRotation = true;

    private void LateUpdate()
    {
        if (followTarget == null)
            return;

        Vector3 pos = followTarget.position;
        pos.y += yOffset;
        transform.position = pos;

        if (lockFlatRotation)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void SetTarget(Transform t) => followTarget = t;
}