using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBS.Gameplay
{
    public class LoadMainMenu : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
