public class BulletTsvRow : ITsvRowWithId
{
    public string Id { get; set; }
    public string DisplayName { get; set; }

    public float Weight { get; set; }

    public string Caliber { get; set; }
    public float DamageMultiplier { get; set; }

    public bool IsDefault { get; set; }
}