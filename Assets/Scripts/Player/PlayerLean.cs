using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerLook))]
public class PlayerLean : PlayerModule
{
    [Header("Lean Settings")]
    [Tooltip("Kąt wychylenia w stopniach.")]
    public float leanAngle = 15f;

    [Tooltip("Przesunięcie kamery podczas wychylenia.")]
    public float leanOffset = 0.2f;

    [Tooltip("Szybkość interpolacji wychylenia.")]
    public float leanSpeed = 5f;
    
    private PlayerInput input;
    private PlayerLook playerLook;

    private Quaternion targetRotation = Quaternion.identity;
    private Quaternion currentRotation = Quaternion.identity;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 currentPosition;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerLook = GetComponent<PlayerLook>();
    }

    private void Start()
    {
        originalPosition = Player.cameraTransform.localPosition;
        targetPosition = originalPosition;
        currentPosition = originalPosition;
    }

    private void Update()
    {
        HandleLeanInput();
        
        currentRotation = Quaternion.Lerp(currentRotation, targetRotation, leanSpeed * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, leanSpeed * Time.deltaTime);
        
        playerLook.ApplyLeanRotation(currentRotation);
        Player.cameraTransform.localPosition = currentPosition;
    }

    private void HandleLeanInput()
    {
        if (input.IsLeanLeftPressed)
        {
            SetLean(leanAngle, -leanOffset);
        }
        else if (input.IsLeanRightPressed)
        {
            SetLean(-leanAngle, leanOffset);
        }
        else
        {
            ResetLean();
        }
    }

    private void SetLean(float angle, float offset)
    {
        targetRotation = Quaternion.Euler(0f, 0f, angle);
        targetPosition = originalPosition + new Vector3(offset, 0f, 0f);
    }

    private void ResetLean()
    {
        targetRotation = Quaternion.identity;
        targetPosition = originalPosition;
    }
}
