using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

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
    public bool IsWhistlePressed { get; private set; }
    public bool IsEscapePressed { get; private set; }
    public bool IsAiming { get; private set; } // Trzymanie PPM
    public bool IsThrowing { get; private set; } // Rzut LPM

    public event System.Action OnStartAiming;
    public event System.Action OnStopAiming;
    public event System.Action OnThrowStone;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(new PlayerActions(this));
    }

    private void OnEnable() => inputActions.Player.Enable();

    private void OnDisable() => inputActions.Player.Disable();

    public void ResetVaultRequest() => IsVaultPressed = false;

    public void ResetEscapePressed() => IsEscapePressed = false;

    private class PlayerActions : IPlayerActions
    {
        private readonly PlayerInput _playerInput;

        public PlayerActions(PlayerInput input) => _playerInput = input;

        public void OnMove(InputAction.CallbackContext context) => _playerInput.MoveInput = context.ReadValue<Vector2>();

        public void OnLook(InputAction.CallbackContext context) => _playerInput.LookInput = context.ReadValue<Vector2>();

        public void OnVault(InputAction.CallbackContext context) => _playerInput.IsVaultPressed = context.performed;

        public void OnCrouch(InputAction.CallbackContext context) => _playerInput.IsCrouchPressed = context.performed;

        public void OnInteract(InputAction.CallbackContext context) => _playerInput.IsInteractPressed = context.performed;

        public void OnLeanLeft(InputAction.CallbackContext context) => _playerInput.IsLeanLeftPressed = context.performed;

        public void OnLeanRight(InputAction.CallbackContext context) => _playerInput.IsLeanRightPressed = context.performed;

        public void OnWhistling(InputAction.CallbackContext context) => _playerInput.IsWhistlePressed = context.performed;

        public void OnCheatSheet(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _playerInput.IsEscapePressed = true;
            }
        }

        public void OnThrow(InputAction.CallbackContext context)
        {
            _playerInput.IsThrowing = context.performed;
            if (context.performed)
            {
                _playerInput.OnThrowStone?.Invoke(); // Wywo³anie akcji rzutu
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            _playerInput.IsAiming = context.performed;

            if (context.performed)
            {
                _playerInput.OnStartAiming?.Invoke(); // Wywo³anie pocz¹tku celowania
            }
            else if (context.canceled)
            {
                _playerInput.OnStopAiming?.Invoke(); // Wywo³anie koñca celowania
            }
        }
    }
}
