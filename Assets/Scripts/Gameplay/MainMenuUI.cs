using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBS.Gameplay
{
    public class MainMenuUI : MonoBehaviour
    {
        public void HostGame()
        {
            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }

        public void JoinGame()
        {
            NetworkManager.Singleton.StartClient();

            SceneManager.LoadScene("Gameplay");
        }
    }
}
