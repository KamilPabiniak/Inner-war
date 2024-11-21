using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private Player _player;
    private PlayerInputActions _inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool LeanLeftPressed { get; private set; } 
    public bool LeanRightPressed { get; private set; } 

    private void Awake()
    {
        _player = GetComponent<Player>();
        _inputActions = new PlayerInputActions();
        _inputActions.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        JumpPressed = _player.InputEnabled && context.performed;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        CrouchPressed = _player.InputEnabled && context.performed;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        InteractPressed = _player.InputEnabled && context.performed;
    }

    public void OnLeanLeft(InputAction.CallbackContext context)
    {
        LeanLeftPressed = _player.InputEnabled && context.performed;
    }

    public void OnLeanRight(InputAction.CallbackContext context)
    {
        LeanRightPressed = _player.InputEnabled && context.performed;
    }
}