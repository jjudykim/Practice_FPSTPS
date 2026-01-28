using UnityEngine;

public interface IAimProvider
{
    Camera AimCamera { get; } // 조준에 사용할 카메라
    Transform Muzzle { get; } // 실제 발사 원점 (총구)

    Ray GetAimRay();

    bool TryGetAimPoint(float maxDistance, LayerMask mask, out Vector3 hitPoint);
}
