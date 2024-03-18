using PRN;
using System;
using TBS.Network;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class NetworkPlayerProcessor : MonoBehaviour, IProcessor<NetworkPlayerInput, NetworkPlayerState>
    {
        [SerializeField]
        private float turnSmoothTime = 0.1f;

        [SerializeField]
        private float movementSpeed = 8f;

        [SerializeField]
        private float jumpHeight = 2.5f;

        [SerializeField]
        private float gravityForce = -9.81f;

        public bool IsPlayerMoving => movement != Vector3.zero;

        public Vector3 movement = Vector3.zero;
        public Vector3 gravity = Vector3.zero;

        private CharacterController controller;

        private float turnSmoothVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        // You need to implement this method
        // Your player logic happens here
        public NetworkPlayerState Process(NetworkPlayerInput input, TimeSpan deltaTime)
        {
            //movement = (Vector3.forward * input.forward + Vector3.right * input.right).normalized * movementSpeed * (float)deltaTime.TotalSeconds;
            //if (controller.isGrounded)
            //{
            //    gravity = Vector3.zero;
            //    if (input.jump)
            //    {
            //        gravity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * -gravityForce) * (float)deltaTime.TotalSeconds;
            //    }
            //}
            //if (gravity.y > 0)
            //{
            //    gravity += Vector3.up * gravityForce * Mathf.Pow((float)deltaTime.TotalSeconds, 2);
            //}
            //else
            //{
            //    gravity += Vector3.up * gravityForce * Mathf.Pow((float)deltaTime.TotalSeconds, 2) * 1.3f;
            //}

            //Debug.Log(movement + gravity);

            //controller.Move(movement + gravity);
            //return new NetworkPlayerState()
            //{
            //    position = transform.position,
            //    movement = movement,
            //    gravity = gravity
            //};

            var movementNormalized = (Vector3.forward * input.forward + Vector3.right * input.right).normalized;

            if (controller.isGrounded)
            {
                gravity = Vector3.zero;
                if (input.jump)
                {
                    gravity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * -gravityForce) * (float)deltaTime.TotalSeconds;
                }
            }
            if (gravity.y > 0)
            {
                gravity += Vector3.up * gravityForce * Mathf.Pow((float)deltaTime.TotalSeconds, 2);
            }
            else
            {
                gravity += Vector3.up * gravityForce * Mathf.Pow((float)deltaTime.TotalSeconds, 2) * 1.3f;
            }

            if (input.forward != 0|| input.right != 0)
            {
                float targetAngle = Mathf.Atan2(movementNormalized.x, movementNormalized.z) * Mathf.Rad2Deg + input.cameraYAngle;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                movement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward * movementSpeed * (float)deltaTime.TotalSeconds;
            }
            else
            {
                movement= Vector3.zero;
            }

            controller.Move(movement + gravity);

            return new NetworkPlayerState()
            {
                position = transform.position,
                movement = movement,
                gravity = gravity
            };
        }

        // You need to implement this method
        // Called when an inconsistency occures
        public void Rewind(NetworkPlayerState state)
        {
            controller.enabled = false;
            transform.position = state.position;
            movement = state.movement;
            gravity = state.gravity;
            controller.enabled = true;
        }
    }
}
