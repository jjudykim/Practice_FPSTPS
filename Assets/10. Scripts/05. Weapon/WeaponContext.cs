using UnityEngine;

public class WeaponContext
{
    public  IAimProvider AimProvider;
    public  Transform Owner;      
    public  bool IsADS;

    public WeaponContext(IAimProvider aimProvider, Transform owner, bool isADS)
    {
        AimProvider = aimProvider;
        Owner = owner;
        IsADS = isADS;
    }
    
    public void SetAimProvider(IAimProvider provider)
    {
        AimProvider = provider;
    }

    public void SetADS(bool isADS)
    {
        IsADS = isADS;
    }
}