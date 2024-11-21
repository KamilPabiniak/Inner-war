using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCrouch))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    [Range(0, 100)] public float crouchSpeedReduction = 50f; 
    public float gravity = 20f;

    [Header("References")]
    public Transform cameraTransform;
    public CharacterController characterController;
    public Transform playerBody;  
    public Rigidbody rigidbody; 

    [Header("Commands")]
    public bool inputEnabled = true;
    public bool gravityEnabled = true;

    public bool InputEnabled => inputEnabled;

    private float _verticalVelocity;
    
    private void Update()
    {
        ApplyGravity();
    }

    public void EnableInput() => inputEnabled = true;
    public void DisableInput() => inputEnabled = false;

    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }
        else if (_verticalVelocity < 0)
        {
            _verticalVelocity = 0f;
        }

        if (!gravityEnabled) return;
        characterController.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
    }
}
