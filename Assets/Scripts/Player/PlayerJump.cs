using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private Player _player;
    private PlayerInput _input;

    private void Start()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!_player.InputEnabled) return;
        HandleJump();
    }

    //NO JUMPING ALLOWED
    private void HandleJump()
    {
        if (_input.JumpPressed && _player.characterController.isGrounded)
        {
            
        }
    }
}