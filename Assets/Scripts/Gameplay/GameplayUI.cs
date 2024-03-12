using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBS.Gameplay {
    public class GameplayUI : MonoBehaviour
    {
        public void LeaveGame()
        {
            NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene("MainMenu");
        }
    }
}