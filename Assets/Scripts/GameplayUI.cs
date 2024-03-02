using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUI : MonoBehaviour
{
    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("MainMenu");
    }
}
