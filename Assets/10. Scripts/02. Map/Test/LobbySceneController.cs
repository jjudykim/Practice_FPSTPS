using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneController : MonoBehaviour
{
    [SerializeField] private string mapSceneName = "TestMapScene";

    public void OnClickStart()
    {
        Managers.Instance.Scene.LoadScene(mapSceneName);
    }
}
