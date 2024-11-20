using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")] 
    public float moveSpeed = 5f;
    
    private Player _player;
    private PlayerInput _input;
    private PlayerCrouch _crouch;

    [Header("Movement Interpolation")]
    public float acceleration = 5f; // Szybkość przyspieszania
    public float deceleration = 5f; // Szybkość zwalniania

    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
        _crouch = GetComponent<PlayerCrouch>();
    }
    
    private void Update()
    {
        if (!_player.InputEnabled) return;

        HandleMovement(_input.MoveInput);
    }

    private void HandleMovement(Vector2 moveInput)
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        targetVelocity = transform.TransformDirection(targetVelocity) * moveSpeed;

        if (_crouch.IsCrouch())
        {
            float reductionFactor = (100f - _player.crouchSpeedReduction) / 100f;
            targetVelocity *= reductionFactor;
        }

        // Interpolacja prędkości (przyspieszenie/zwalnianie)
        if (moveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        _player.characterController.Move(currentVelocity * Time.deltaTime);
    }
}