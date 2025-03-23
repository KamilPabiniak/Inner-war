using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class PlayerInput : PlayerModule
{
    private PlayerInputActions _inputActions;

    // Analog input values
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    // Button states (polled)
    public bool IsVaultPressed { get; private set; }
    public bool IsCrouchPressed { get; private set; }
    public bool IsInteractPressed { get; internal set; }
    public bool IsLeanLeftPressed { get; private set; }
    public bool IsLeanRightPressed { get; private set; }
    public bool IsWhistlePressed { get; private set; }
    public bool IsAiming { get; private set; }
    private bool IsSettingsPanelButtonPressed { get; set; }

    // Events for actions that require immediate reaction
    public event Action OnStartAiming;
    public event Action OnStopAiming;
    public event Action OnThrowStone;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.SetCallbacks(new PlayerActions(this));
    }

    private void OnEnable() => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    // Reset methods if needed
    public void ResetVaultRequest() => IsVaultPressed = false;
    public void ResetSettingsButtonPressed() => IsSettingsPanelButtonPressed = false;
    
    private void LateUpdate()
    {
        if (!IsSettingsPanelButtonPressed) return;
        GameEvents.onTogglePanel?.Invoke();
        ResetSettingsButtonPressed();
    }
    
    public void BlockAllInputsExceptSettingsPanel(bool block)
    {
        var actionMap = _inputActions.Player.Get();
        foreach (var action in actionMap.actions)
        {
            if (action.name != "SettingsPanel")
            {
                if (block)
                    action.Disable();
                else
                    action.Enable();
            }
        }
    }


    private class PlayerActions : IPlayerActions
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
            _playerInput.IsVaultPressed = context.ReadValueAsButton();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _playerInput.IsCrouchPressed = context.ReadValueAsButton();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                _playerInput.IsInteractPressed = true;
            }
            else if(context.phase == InputActionPhase.Canceled)
            {
                _playerInput.IsInteractPressed = false;
            }
        }


        public void OnLeanLeft(InputAction.CallbackContext context)
        {
            _playerInput.IsLeanLeftPressed = context.ReadValueAsButton();
        }

        public void OnLeanRight(InputAction.CallbackContext context)
        {
            _playerInput.IsLeanRightPressed = context.ReadValueAsButton();
        }

        public void OnWhistling(InputAction.CallbackContext context)
        {
            _playerInput.IsWhistlePressed = context.ReadValueAsButton();
        }

        public void OnSettingsPanel(InputAction.CallbackContext context)
        {
            _playerInput.IsSettingsPanelButtonPressed = context.ReadValueAsButton();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    _playerInput.IsAiming = true;
                    _playerInput.OnStartAiming?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    _playerInput.IsAiming = false;
                    _playerInput.OnStopAiming?.Invoke();
                    break;
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Performed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnThrow(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    _playerInput.OnThrowStone?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    break;
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Performed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
