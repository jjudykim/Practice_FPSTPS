using UnityEngine;

public class WeaponSlot
{
    public string WeaponId;
    public int CurrentAmmo;
}

public class PlayerWeaponInventory : MonoBehaviour
{
    [SerializeField] private string[] weaponIds = new string[2] { "Rifle_01", "Pistol_01" };
    public string GetWeaponId(int slotIndex) => (slotIndex >= 0 && slotIndex < weaponIds.Length) ? weaponIds[slotIndex] : null;
}

