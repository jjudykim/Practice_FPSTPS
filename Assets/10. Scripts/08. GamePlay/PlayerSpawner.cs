using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (Player.IsInitialized)
            return;

        Instantiate(playerPrefab);
    }
}