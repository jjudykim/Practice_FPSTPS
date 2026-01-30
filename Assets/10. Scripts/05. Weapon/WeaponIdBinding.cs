using UnityEngine;

public class WeaponIdBinding : MonoBehaviour
{
    [field: SerializeField] public string WeaponId { get; private set; }
    [field: SerializeField] public string BulletId { get; private set; }
}