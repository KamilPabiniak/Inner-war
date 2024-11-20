using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private Player _player;
    private PlayerInput _input;
    private float verticalVelocity;

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

    private void HandleJump()
    {
        if (_input.JumpPressed && _player.characterController.isGrounded)
        {
            verticalVelocity = _player.jumpForce;
        }

        verticalVelocity -= _player.gravity * Time.deltaTime;
        _player.characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
}