using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : PlayerModule
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsVaultPressed { get; private set; }
    public bool IsCrouchPressed { get; private set; }
    public bool IsInteractPressed { get; private set; }
    public bool IsLeanLeftPressed { get; private set; }
    public bool IsLeanRightPressed { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(new PlayerActions(this));
    }

    private void OnEnable() => inputActions.Player.Enable();

    private void OnDisable() => inputActions.Player.Disable();
    
    public void ResetVaultRequest() => IsVaultPressed = false;


    private class PlayerActions : PlayerInputActions.IPlayerActions
    {
        private readonly PlayerInput _playerInput;

        public PlayerActions(PlayerInput input) => _playerInput = input;

        public void OnMove(InputAction.CallbackContext context)
        {
            _playerInput.MoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _playerInput.LookInput = context.ReadValue<Vector2>();
        }

        public void OnVault(InputAction.CallbackContext context)
        {
            _playerInput.IsVaultPressed = context.performed;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _playerInput.IsCrouchPressed = context.performed;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            _playerInput.IsInteractPressed = context.performed;
        }

        public void OnLeanLeft(InputAction.CallbackContext context)
        {
            _playerInput.IsLeanLeftPressed = context.performed;
        }

        public void OnLeanRight(InputAction.CallbackContext context)
        {
            _playerInput.IsLeanRightPressed = context.performed;
        }
    }
}
