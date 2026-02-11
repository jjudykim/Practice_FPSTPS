using UnityEngine;

public class EndPointTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private RoomControllerBase room;
    [SerializeField] private GameObject visualRoot;
    
    private Collider triggerCol;

    private void Awake()
    {
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        // 룸 참조가 비어있으면 부모에서 찾아보기
        if (room == null)
            room = GetComponentInParent<RoomControllerBase>();

        if (room == null)
            Debug.LogError("[EndPointTrigger] CombatRoomController reference is missing.");
        
        if (room is CombatRoomController)
            SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (triggerCol != null)
            triggerCol.enabled = active;

        if (visualRoot != null)
            visualRoot.SetActive(active);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (room == null)
            return;

        if (other.CompareTag("Player"))
        {
            room.OnPlayerReachedEndPoint();
        }
    }
}