using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float verticalClamp = 85f;

    [Header("Camera Bobbing")]
    public bool enableBobbing = true; 
    public float bobbingSpeed = 10f; 
    public float bobbingStrenght = 0.05f; 
    
    private Player _player;
    private PlayerInput _input;
    private float xRotation = 0f;
    private float bobbingOffset = 0f;
    private float bobbingTimer = 0f;
    private Quaternion leanRotation = Quaternion.identity;

    private void Start()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
        LockCursor();
    }

    private void Update()
    {
        HandleCursor();

        if (!_player.InputEnabled || Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleLook(_input.LookInput);

        if (enableBobbing)
            HandleCameraBobbing(_input.MoveInput);
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
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

        Quaternion cameraRotation = Quaternion.Euler(xRotation, 0f, 0f);
        _player.cameraTransform.localRotation = cameraRotation * leanRotation;
        _player.playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleCameraBobbing(Vector2 moveInput)
    {
        if (moveInput.magnitude > 0.1f) 
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            bobbingOffset = Mathf.Sin(bobbingTimer) * bobbingStrenght;
        }
        else
        {
            bobbingTimer = 0;
            bobbingOffset = Mathf.Lerp(bobbingOffset, 0, Time.deltaTime * bobbingSpeed);
        }

        Vector3 cameraPosition = _player.cameraTransform.localPosition;
        cameraPosition.y += bobbingOffset;
        _player.cameraTransform.localPosition = cameraPosition;
    }

    public void ApplyLeanRotation(Quaternion leanRot)
    {
        leanRotation = leanRot;
    }
}
