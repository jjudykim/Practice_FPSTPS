public class ItemTsvRow : ITsvRowWithId
{
    public string Id { get; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string Type { get; set; }
    public string Slot { get; set; }
    public float Weight { get; set; }
    public int StackLimit { get; set; }
    public string EffectType { get; set; }
    public float EffectValue { get; set; }
    public float EffectDuration { get; set; }
    public float EffectSecRate { get; set; }
}