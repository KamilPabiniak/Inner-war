using UnityEngine;

public class PlayerLook : PlayerModule
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float verticalClamp = 85f;

    [Header("Camera Bobbing")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 10f;
    public float bobbingStrength = 0.05f;
    
    [Header("Camera Settings")]
    [Tooltip("Podstawowa wysokość kamery względem punktu gracza")]
    public float cameraHeight = 1.6f;
    
    private PlayerInput input;
    private float xRotation;
    private float bobbingOffset;
    private float bobbingTimer;

    private Quaternion leanRotation = Quaternion.identity; 

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        LockCursor();
    }

    private void Update()
    {
        if (Player.state == Player.State.Walking)
        {
            HandleCursor();
            HandleLook(input.LookInput);

            if (enableBobbing)
                HandleCameraBobbing(input.MoveInput);
        }
        else if (Player.state == Player.State.Climbing)
        {
            HandleClimbingLook();
        }
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);
        
        Player.cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0) * leanRotation;
        Player.CharacterController.transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCameraBobbing(Vector2 moveInput)
    {
        if (moveInput.magnitude > 0.1f)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            bobbingOffset = Mathf.Sin(bobbingTimer) * bobbingStrength;
        }
        else
        {
            bobbingTimer = 0;
            bobbingOffset = Mathf.Lerp(bobbingOffset, 0, Time.deltaTime * bobbingSpeed);
        }

        Vector3 cameraPosition = Player.cameraTransform.localPosition;
        cameraPosition.y = cameraHeight + bobbingOffset;
        Player.cameraTransform.localPosition = cameraPosition;
    }
    
    private void HandleClimbingLook()
    {
        Player.cameraTransform.localRotation = Quaternion.identity;
        xRotation = 0;
    }

    /// <summary>
    /// Ustawia dodatkową rotację wychylenia.
    /// </summary>
    /// <param name="rotation">Rotacja do zastosowania.</param>
    public void ApplyLeanRotation(Quaternion rotation)
    {
        leanRotation = rotation;
    }
}
