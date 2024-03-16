using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBS.Gameplay {
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text FPSText;

        [SerializeField]
        private float _hudRefreshRate = 1f;

        private float _timer;

        private void Update()
        {
            if (Time.unscaledTime > _timer)
            {
                int fps = (int)(1f / Time.unscaledDeltaTime);
                FPSText.text = fps+ " FPS";
                _timer = Time.unscaledTime + _hudRefreshRate;
            }
        }

        public void LeaveGame()
        {
            NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene("MainMenu");
        }
    }
}