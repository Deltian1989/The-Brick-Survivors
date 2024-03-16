using TBS.CameraUtils;
using TBS.Gameplay.ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField]
        private CameraHolder cameraHolderPrefab;

        [SerializeField]
        private NetworkCharacterMovement m_NetworkCharacterMovement;

        [SerializeField]
        private PlayerInputReader playerInputReader;

        [SerializeField]
        private Transform lookAt;

        public bool IsPlayerMoving => m_moveInput != Vector2.zero;

        private CameraHolder currentPlayerCamera;

        Vector2 m_moveInput;

        public override void OnNetworkSpawn()
        {
            if (IsLocalPlayer)
            {
                currentPlayerCamera = Instantiate(cameraHolderPrefab, transform.position, Quaternion.identity);

                DontDestroyOnLoad(currentPlayerCamera);

                currentPlayerCamera.SetCameraLookAtTransform(lookAt);

                playerInputReader.WalkEvent += OnPlayerWalking;
                playerInputReader.Enable();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsLocalPlayer)
            {
                Destroy(currentPlayerCamera.gameObject);

                playerInputReader.WalkEvent -= OnPlayerWalking;
                playerInputReader.Disable();
            }
        }

        private void OnPlayerWalking(Vector2 moveInput)
        {
            m_moveInput = moveInput;
        }

        private void Update()
        {
            if(IsClient && IsLocalPlayer)
            {
                float cameraYAngle = currentPlayerCamera.Camera.transform.eulerAngles.y;

                m_NetworkCharacterMovement.ProcessLocalPlayerMovement(m_moveInput, cameraYAngle);
            }
            else
            {
                m_NetworkCharacterMovement.ProcessSimulatedPlayerMovement();
            }
        }
    }
}
