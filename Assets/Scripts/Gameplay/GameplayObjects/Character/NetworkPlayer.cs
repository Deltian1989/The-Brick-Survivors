using PRN;
using System;
using System.Collections.Generic;
using TBS.CameraUtils;
using TBS.Network;
using Unity.Netcode;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class NetworkPlayer : NetworkBehaviour
    {
        private const float PROCESS_TICK_RATE = 1 / 120f;
        private const float NETWORK_UPDATE_TICK_RATE = 1 / 10f;

        public CameraHolder currentPlayerCamera;

        public Vector3 position
        {
            get => _position.Value;
            private set
            {
                if (IsServer)
                    _position.Value = value;
            }
        }

        [SerializeField]
        private CameraHolder cameraHolderPrefab;

        private Ticker processTicker = new Ticker(TimeSpan.FromSeconds(PROCESS_TICK_RATE));
        private Ticker networkUpdateTicker = new Ticker(TimeSpan.FromSeconds(NETWORK_UPDATE_TICK_RATE));

        private NetworkPlayerProcessor processor;
        private NetworkPlayerInputProvider inputProvider;
        private NetworkPlayerStateConsistencyChecker consistencyChecker;

        private NetworkHandler<NetworkPlayerInput, NetworkPlayerState> networkHandler;

        private NetworkVariable<Vector3> _position = new NetworkVariable<Vector3>();

        private List<NetworkPlayerInput> pendingOwnerInputs = new List<NetworkPlayerInput>();
        private List<NetworkPlayerInputState> pendingServerInputStates = new List<NetworkPlayerInputState>();

        [SerializeField]
        private Transform lookAt;

        private void Awake()
        {
            processor = GetComponent<NetworkPlayerProcessor>();
            inputProvider = GetComponent<NetworkPlayerInputProvider>();
            consistencyChecker = GetComponent<NetworkPlayerStateConsistencyChecker>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkRole role;
            if (IsServer)
            {
                role = IsOwner ? NetworkRole.HOST : NetworkRole.SERVER;
            }
            else
            {
                role = IsOwner ? NetworkRole.OWNER : NetworkRole.GUEST;
            }
            networkHandler = new NetworkHandler<NetworkPlayerInput, NetworkPlayerState>(
                role: role,
                ticker: processTicker,
                processor: processor,
                inputProvider: inputProvider,
                consistencyChecker: consistencyChecker,
                stateSyncPolicy: new StateSyncPolicy(withPrediction: true)
            );

            networkHandler.onSendInputToServer += SendInputToServer;
            networkHandler.onSendInputStateToClient += SendInputStateToClient;
            networkHandler.onState += OnState;

            networkHandler.onTickerHackerDetected += () => {
                NetworkObject.Despawn();
            };

            networkUpdateTicker.onTick += () => {
                if (pendingOwnerInputs.Count > 0)
                {
                    SendInputsServerRpc(pendingOwnerInputs.ToArray());
                    pendingOwnerInputs.Clear();
                }
                if (pendingServerInputStates.Count > 0)
                {
                    SendInputStateClientRpc(pendingServerInputStates.ToArray());
                    pendingServerInputStates.Clear();
                }
            };

            if (IsOwner)
            {
                currentPlayerCamera = Instantiate(cameraHolderPrefab, transform.position, Quaternion.identity);
                DontDestroyOnLoad(currentPlayerCamera);

                currentPlayerCamera.SetCameraLookAtTransform(lookAt);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                Destroy(currentPlayerCamera.gameObject);
            }
        }

        private void FixedUpdate()
        {
            processTicker.OnTimePassed(TimeSpan.FromSeconds(Time.fixedDeltaTime));
            networkUpdateTicker.OnTimePassed(TimeSpan.FromSeconds(Time.fixedDeltaTime));
        }

        private void SendInputToServer(NetworkPlayerInput input)
        {
            pendingOwnerInputs.Add(input);
        }

        [ServerRpc]
        private void SendInputsServerRpc(NetworkPlayerInput[] inputs)
        {
            foreach (NetworkPlayerInput input in inputs)
                networkHandler.OnOwnerInputReceived(input);
        }

        private void SendInputStateToClient(NetworkPlayerInput input, NetworkPlayerState state)
        {
            pendingServerInputStates.Add(new NetworkPlayerInputState() { input = input, state = state });
        }

        [ClientRpc]
        private void SendInputStateClientRpc(NetworkPlayerInputState[] inputStates)
        {
            foreach (NetworkPlayerInputState inputState in inputStates)
                networkHandler.OnServerInputStateReceived(inputState.input, inputState.state);
        }

        private void OnState(NetworkPlayerState state)
        {
            // Do whatever you need
            // This method is called on the server or the host when they generate a state
            // on the owner when it predicts a state and during its reconciliation
            // on the client when it receives a state from the server
            if (IsServer)
                position = state.position;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            networkHandler.Dispose();
        }
    }
}
