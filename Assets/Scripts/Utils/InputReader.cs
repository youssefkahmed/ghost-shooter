using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Ghost.Utils
{
    public class InputReader : ScriptableObject, IPlayerActions, IInputReader
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Dash = delegate { };
        public event UnityAction Attack = delegate { };
        public event UnityAction<RaycastHit> Click = delegate { };

        private PlayerInputActions _inputActions;
        private Camera _mainCamera;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void EnablePlayerActions()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Enable();
        }
        
        public bool IsJumpKeyPressed()
        {
            return _inputActions.Player.Jump.IsPressed();
        }
    
        public Vector2 Direction => _inputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookDirection => _inputActions.Player.Look.ReadValue<Vector2>();
        
        public void OnMove(InputAction.CallbackContext context)
        {
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look?.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (IsDeviceMouse(context))
                {
                    Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 100))
                    {
                        Click?.Invoke(hit);
                    }
                }
            }
        }

        public void OnMouseControlCamera(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    EnableMouseControlCamera?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    DisableMouseControlCamera?.Invoke();
                    break;
            }
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Dash?.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Dash?.Invoke(false);
                    break;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Jump?.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump?.Invoke(false);
                    break;
            }
        }
        
        private static bool IsDeviceMouse(InputAction.CallbackContext context)
        {
            return context.control.device.name == "Mouse";
        }
    }
    
    public interface IInputReader
    {
        Vector2 Direction { get; }
        void EnablePlayerActions();
    }
}
