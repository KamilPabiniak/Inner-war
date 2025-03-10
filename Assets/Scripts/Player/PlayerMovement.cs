using UnityEngine;

public class PlayerMovement : PlayerModule
{
    [Header("Settings")] 
    public float moveSpeed = 5f;
    public float climbSpeed = 3f;
    [Tooltip("Redukcja prędkości podczas kucania (w %).")]
    [Range(0, 100)] public float crouchSpeedReduction = 50f;

    private PlayerInput _input;
    private bool _isCrouch;

    [Header("Movement Interpolation")]
    public float acceleration = 5f; 
    public float deceleration = 5f;

    public Vector3 currentVelocity { get; private set; } = Vector3.zero;

    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (Player.state == Player.State.Walking)
        {
            HandleWalking(_input.MoveInput);
            HandleCrouch();
        }
        else if (Player.state == Player.State.Climbing)
        {
            HandleClimbing(_input.MoveInput);
        }
    }

    public void ForceCrouch()
    {
        _isCrouch = true;
    }

    public void DisableCrouch()
    {
        _isCrouch = false;
    }

    private void HandleWalking(Vector2 moveInput)
    {
        if (!Player.characterController.enabled) return;
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        targetVelocity = transform.TransformDirection(targetVelocity) * moveSpeed;

        if (_isCrouch)
        {
            float reductionFactor = (100f - crouchSpeedReduction) / 100f;
            targetVelocity *= reductionFactor;
        }
        
        if (moveInput.magnitude > 0.1f)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        Player.characterController.Move(currentVelocity * Time.deltaTime);
    }
    
    private void HandleCrouch()
    {
        _isCrouch = _input.IsCrouchPressed;
        Player.characterController.height = _input.IsCrouchPressed ? Player.crouchHeight : Player.standingHeight;
    }

    private void HandleClimbing(Vector2 moveInput)
    {
        if (!Player.characterController.enabled) return;

        Vector3 climbDirection = Vector3.up * (moveInput.y * climbSpeed);
        Vector3 fixedHorizontalPosition = transform.position;
        fixedHorizontalPosition.y = Player.characterController.transform.position.y;
        Player.characterController.transform.position = fixedHorizontalPosition;

        Player.characterController.Move(climbDirection * Time.deltaTime);
    }
}
