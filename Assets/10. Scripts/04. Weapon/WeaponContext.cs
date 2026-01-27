using UnityEngine;

public struct WeaponContext
{
    public readonly IAimProvider AimProvider;
    public readonly Transform Owner;

    public readonly bool IsADS;

    public WeaponContext(IAimProvider aimProvider, Transform owner, bool isADS)
    {
        AimProvider = aimProvider;
        Owner = owner;
        IsADS = isADS;
    }
}