public class SessionContext
{
    public SessionInventory Inventory { get; private set; } = new SessionInventory();
    public float SessionStartTime { get; private set; }
    public int TotalKills { get; private set; }

    public void StartSession(int bagSlots, float bagWeight)
    {
        Inventory.Clear();
        Inventory.SetupBackpack(bagSlots, bagWeight);
        SessionStartTime = UnityEngine.Time.time;
        TotalKills = 0;
    }

    public void AddKill() => TotalKills++;
}