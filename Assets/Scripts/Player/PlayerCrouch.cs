using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    private Player _player;
    private PlayerInput _input;
    private bool _isCrouch;

    private void Start()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!_player.InputEnabled) return;
        HandleCrouch();
    }

    private void HandleCrouch()
    {
        _player.characterController.height = _input.IsCrouchPressed ? _player.crouchHeight : _player.standingHeight;
        _isCrouch = _input.IsCrouchPressed;
    }

    public bool IsCrouch() => _isCrouch;
}