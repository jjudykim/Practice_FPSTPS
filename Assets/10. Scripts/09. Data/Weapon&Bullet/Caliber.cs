public enum Caliber
{
    None = 0,
    Pistol_45ACP,
    Pistol_9mm,
    Rifle_556,
    Rifle_762,
    SniperCal,
    ShotGun_12G,
}

public static class CaliberExtensions
{
    public static bool IsCompatibleWith(this Caliber weaponCaliber, Caliber bulletCaliber)
    {
        if (weaponCaliber == Caliber.None)
            return false;

        if (bulletCaliber == Caliber.None)
            return false;

        return weaponCaliber == bulletCaliber;
    }
}