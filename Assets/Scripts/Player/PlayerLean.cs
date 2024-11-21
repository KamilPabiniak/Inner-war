using UnityEngine;

public class PlayerLean : MonoBehaviour
{
    private Player player;
    private PlayerInput input;

    [Header("Lean Settings")]
    public float leanAngle = 15f; // Kąt wychylenia
    public float leanOffset = 0.2f; // Przesunięcie kamery podczas wychylenia
    public float leanSpeed = 5f; // Szybkość interpolacji

    private Quaternion targetRotation = Quaternion.identity; // Docelowa rotacja kamery
    private Quaternion currentRotation = Quaternion.identity; // Aktualna rotacja kamery

    private Vector3 originalPosition; // Początkowa pozycja kamery
    private Vector3 targetPosition;   // Docelowa pozycja kamery
    private Vector3 currentPosition;  // Aktualna pozycja kamery

    private PlayerLook playerLook;

    private void Start()
    {
        player = GetComponent<Player>();
        input = GetComponent<PlayerInput>();
        playerLook = GetComponent<PlayerLook>();

        originalPosition = player.cameraTransform.localPosition; // Ustawienie początkowej pozycji kamery
        targetPosition = originalPosition;
        currentPosition = originalPosition;
    }

    private void Update()
    {
        if (!player.InputEnabled) return;

        HandleLean();

        // Płynna interpolacja rotacji i pozycji
        currentRotation = Quaternion.Lerp(currentRotation, targetRotation, leanSpeed * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, leanSpeed * Time.deltaTime);

        // Aktualizacja kamery
        playerLook.ApplyLeanRotation(currentRotation);
        player.cameraTransform.localPosition = currentPosition;
    }

    private void HandleLean()
    {
        if (input.LeanLeftPressed)
        {
            // Wychylenie w lewo
            SetLean(leanAngle, -leanOffset);
        }
        else if (input.LeanRightPressed)
        {
            // Wychylenie w prawo
            SetLean(-leanAngle, leanOffset);
        }
        else
        {
            // Reset wychylenia, jeśli żaden przycisk nie jest trzymany
            ResetLean();
        }
    }

    private void SetLean(float angle, float offset)
    {
        // Ustawiamy docelową rotację i przesunięcie
        targetRotation = Quaternion.Euler(0f, 0f, angle);
        targetPosition = originalPosition + new Vector3(offset, 0f, 0f); // Przesunięcie w osi X
    }

    private void ResetLean()
    {
        // Resetujemy rotację i pozycję do oryginalnej
        targetRotation = Quaternion.identity;
        targetPosition = originalPosition;
    }
}
