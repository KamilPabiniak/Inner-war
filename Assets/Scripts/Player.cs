using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCrouch))]
[RequireComponent(typeof(PlayerJump))]
public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    [Range(0, 100)] public float crouchSpeedReduction = 50f; 
    public float jumpForce = 8f;
    public float gravity = 20f;
   
    [Header("References")]
    public Transform cameraTransform;
    public CharacterController characterController;
    public Transform playerBody;  

    [Header("Commands")]
    public bool inputEnabled = true;

    public bool InputEnabled => inputEnabled;

    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }
}