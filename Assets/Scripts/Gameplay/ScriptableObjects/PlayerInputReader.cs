using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TBS.Gameplay.Input;
using static TBS.Gameplay.Input.PlayerControls;

namespace TBS.Gameplay.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerInputReader", menuName = "Input/Player Input Reader")]
    public class PlayerInputReader : ScriptableObject, IPlayerMoveActions
    {
        private PlayerControls controls;

        public event Action<Vector2> WalkEvent;

        private void Init()
        {
            if (controls == null)
            {
                controls = new PlayerControls();

                controls.PlayerMove.SetCallbacks(this);
            }
        }

        public void Enable()
        {
            Init();

            controls.PlayerMove.Enable();
        }

        public void Disable()
        {
            Init();

            controls.PlayerMove.Disable();
        }

        public void OnMoveAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                WalkEvent?.Invoke(context.action.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                WalkEvent?.Invoke(Vector2.zero);
            }
        }
    }
}
