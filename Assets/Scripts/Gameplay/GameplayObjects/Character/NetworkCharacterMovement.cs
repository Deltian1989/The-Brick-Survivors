using System;
using System.Collections.Generic;
using System.Linq;
using TBS.Network;
using Unity.Netcode;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class NetworkCharacterMovement : NetworkBehaviour
    {
        [SerializeField]
        private float m_speed = 6f;

        [SerializeField]
        private float turnSmoothTime = 0.1f;

        [SerializeField]
        private float positionErrorThreshold = 1;

        [SerializeField]
        private float gravity = -9f;

        [SerializeField]
        private CharacterController characterController;

        [SerializeField] private MeshFilter m_meshFilter;

        [SerializeField] private Color m_Color;

        public bool IsMoving => m_isMoving;

        [HideInInspector]
        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public TransformState m_previousState;

        private bool m_isMoving;
        private int m_tick = 0;
        private float m_tickRate = 1f / 60f;
        private float m_tickDeltaTime = 0f;
        private float turnSmoothVelocity;

        private const int BUFFER_SIZE = 1024;
        private InputState[] m_inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] m_transformStates = new TransformState[BUFFER_SIZE];

        private void OnEnable()
        {

            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, float cameraYAngle)
        {
            m_tickDeltaTime += Time.deltaTime;
            if (m_tickDeltaTime > m_tickRate)
            {
                int bufferIndex = m_tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRpc(m_tick, movementInput, cameraYAngle);
                    MovePlayer(movementInput, cameraYAngle);

                    SaveState(movementInput, cameraYAngle, bufferIndex);
                }
                else
                {
                    MovePlayer(movementInput, cameraYAngle);

                    TransformState state = new TransformState
                    {
                        tick = m_tick,
                        position = transform.position,
                        rotation = transform.rotation,
                        hasStartedMoving = movementInput != Vector2.zero
                    };

                    SaveState(movementInput, cameraYAngle, bufferIndex);

                    m_previousState = ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }

                m_tickDeltaTime -= m_tickRate;
                m_tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            m_tickDeltaTime += Time.deltaTime;
            if (m_tickDeltaTime > m_tickRate)
            {
                if (ServerTransformState.Value != null)
                {
                    transform.position = ServerTransformState.Value.position;
                    transform.rotation = ServerTransformState.Value.rotation;

                    m_isMoving = ServerTransformState.Value.hasStartedMoving;
                }

                m_tickDeltaTime -= m_tickRate;
                m_tick++;
            }
        }

        [ServerRpc]
        private void MovePlayerServerRpc(int tick, Vector2 movementInput, float cameraYAngle)
        {
            MovePlayer(movementInput, cameraYAngle);

            TransformState state = new TransformState
            {
                tick = tick,
                position = transform.position,
                rotation = transform.rotation,
                hasStartedMoving = movementInput != Vector2.zero
            };

            m_previousState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }

        private void SaveState(Vector2 movementInput, float cameraYAngle, int bufferIndex)
        {
            InputState inputState = new InputState
            {
                tick = m_tick,
                movementInput = movementInput,
                cameraYAngle = cameraYAngle
            };

            TransformState transformState = new TransformState
            {
                tick = m_tick,
                position = transform.position,
                rotation = transform.rotation,
                hasStartedMoving = movementInput != Vector2.zero
            };

            m_inputStates[bufferIndex] = inputState;
            m_transformStates[bufferIndex] = transformState;
        }

        private void OnServerStateChanged(TransformState previousValue, TransformState serverState)
        {
            if (!IsLocalPlayer)
                return;

            if (m_previousState == null)
            {
                m_previousState = serverState;
            }

            TransformState calculatedState = m_transformStates.First(localState => localState.tick == serverState.tick);

            // if so, we are out of sync with the server, we must reconciliate the client position with the actual server position
            if (Vector3.Distance(calculatedState.position, serverState.position) > positionErrorThreshold)
            {
                Debug.Log("Correcting client position");
                // Teleport the player to the server position

                TeleportPlayer(serverState);

                // Replay the inputs that happened after

                IEnumerable<InputState> inputs = m_inputStates.Where(input => input.tick > serverState.tick);

                inputs = from input in inputs orderby input.tick select input;

                foreach (var inputState in inputs)
                {
                    MovePlayer(inputState.movementInput, inputState.cameraYAngle);

                    TransformState newTransformState = new TransformState
                    {
                        tick = inputState.tick,
                        position = transform.position,
                        rotation = transform.rotation,
                        hasStartedMoving = inputState.movementInput != Vector3.zero

                    };

                    for (int i = 0; i < m_transformStates.Length; i++)
                    {
                        if (m_transformStates[i].tick == inputState.tick)
                        {
                            m_transformStates[i] = newTransformState;
                            break;
                        }
                    }
                }
            }
        }

        private void TeleportPlayer(TransformState state)
        {
            characterController.enabled = false;

            transform.position = state.position;
            transform.rotation = state.rotation;

            characterController.enabled = true;

            for (int i = 0; i < m_transformStates.Length; i++)
            {
                if (m_transformStates[i].tick == state.tick)
                {
                    m_transformStates[i] = state;
                    break;
                }
            }
        }

        private void MovePlayer(Vector2 movementInput, float cameraYAngle)
        {
            if (movementInput != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraYAngle;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                Vector3 movement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

                movement.y = 0;

                if (!characterController.isGrounded)
                {
                    movement.y = gravity;
                }

                characterController.Move(movement * m_speed * m_tickRate);

                m_isMoving = true;
            }
            else
            {
                m_isMoving = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (ServerTransformState.Value != null)
            {
                Gizmos.color = m_Color;

                Gizmos.DrawMesh(m_meshFilter.mesh, ServerTransformState.Value.position);
            }
        }
    }
}
