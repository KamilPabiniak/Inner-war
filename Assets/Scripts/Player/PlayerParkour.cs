using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerParkour : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public LayerMask vaultLayer; // Warstwa obiektów, na które można się wspinać
    private CharacterController controller;

    [Header("Climbing Settings")]
    public float climbSpeed = 3f;      // Prędkość wspinania
    public float vaultDistance = 1.5f; // Maksymalny dystans do przeszkody
    public float climbHeight = 2f;    // Maksymalna wysokość przeszkody
    public float playerRadius = 0.5f; // Promień wokół gracza do wykrywania przeszkód
    public float ledgeOffset = 0.1f;  // Odległość od górnej krawędzi obiektu

    private PlayerInput inputHandler;
    private bool isClimbing = false;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isClimbing && inputHandler.IsVaultPressed)
        {
            TryVault();
            inputHandler.ResetVaultRequest();
        }
    }

    /// <summary>
    /// Próba wykrycia przeszkody i zainicjowania wspinaczki.
    /// </summary>
    private void TryVault()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit firstHit, vaultDistance, vaultLayer))
        {
            Debug.Log("Obstacle detected!");

            // Szukamy miejsca na krawędzi przeszkody
            Vector3 climbStart = firstHit.point + (cameraTransform.forward * playerRadius) + (Vector3.up * 0.6f * climbHeight);

            if (Physics.Raycast(climbStart, Vector3.down, out RaycastHit secondHit, climbHeight))
            {
                Debug.Log("Valid climb position found!");
                StartCoroutine(Climb(secondHit.point));
            }
            else
            {
                Debug.Log("No valid climb point found!");
            }
        }
    }

    /// <summary>
    /// Coroutine obsługująca wspinaczkę gracza.
    /// </summary>
    /// <param name="targetPosition">Pozycja, na którą gracz ma się wspiąć.</param>
    private IEnumerator Climb(Vector3 targetPosition)
    {
        isClimbing = true;
        controller.enabled = false; // Wyłączenie CharacterControllera

        Vector3 startPosition = transform.position;
        Vector3 finalPosition = new Vector3(targetPosition.x, targetPosition.y + ledgeOffset, targetPosition.z);
        float distance = Vector3.Distance(startPosition, finalPosition);
        float climbDuration = distance / climbSpeed; // Długość wspinania zależna od odległości
        float elapsedTime = 0f;

        while (elapsedTime < climbDuration)
        {
            // Płynne przesuwanie gracza
            transform.position = Vector3.Lerp(startPosition, finalPosition, elapsedTime / climbDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Po wspinaczce przywracamy kontrolę graczowi
        transform.position = finalPosition;
        controller.enabled = true;
        isClimbing = false;
    }
}
