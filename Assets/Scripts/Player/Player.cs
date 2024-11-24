using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    [Tooltip("Redukcja prędkości podczas kucania (w %).")]
    [Range(0, 100)] public float crouchSpeedReduction = 50f;
    public float gravity = 20f;

    [Header("References")]
    public Transform cameraTransform;
    public CharacterController characterController;

    public Transform CameraTransform => cameraTransform;
    public CharacterController CharacterController => characterController;

    private float verticalVelocity;

    public bool InputEnabled { get; private set; } = true;
    public bool GravityEnabled { get; set; } = true;

    private void Update()
    {
        ApplyGravity();
    }

    public void EnableInput() => InputEnabled = true;
    public void DisableInput() => InputEnabled = false;

    private void ApplyGravity()
    {
        if (!characterController.enabled) return;
        if (!characterController.isGrounded)
            verticalVelocity -= gravity * Time.deltaTime;
        else if (verticalVelocity < 0)
            verticalVelocity = 0f;

        if (!GravityEnabled) return;

        Vector3 gravityMovement = Vector3.up * (verticalVelocity * Time.deltaTime);
        characterController.Move(gravityMovement);
    }
}