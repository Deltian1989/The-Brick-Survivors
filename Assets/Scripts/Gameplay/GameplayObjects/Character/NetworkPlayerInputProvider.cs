using PRN;
using TBS.Gameplay.ScriptableObjects;
using TBS.Network;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class NetworkPlayerInputProvider : MonoBehaviour, IInputProvider<NetworkPlayerInput>
    {
        public bool pendingJump = false;

        [SerializeField]
        private NetworkPlayer networkPlayer;

        [SerializeField]
        private PlayerInputReader playerInputReader;

        private NetworkPlayerInput input;

        private void Awake()
        {
            playerInputReader.WalkEvent += OnPlayerMoving;
            playerInputReader.Enable();
            pendingJump = false;
        }

        private void Update()
        {
            if (networkPlayer.currentPlayerCamera != null)
            {
                float cameraYAngle = networkPlayer.currentPlayerCamera.Camera.transform.eulerAngles.y;

                input.cameraYAngle = cameraYAngle;
            }
        }

        private void OnPlayerMoving(Vector2 move)
        {
            input.forward = move.y;
            input.right = move.x;
        }

        // You need to implement this method
        public NetworkPlayerInput GetInput()
        {
            input.jump = pendingJump;
            pendingJump = false;
            return input;
        }
    }
}
