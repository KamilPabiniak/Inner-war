using System.Collections;
using UnityEngine;

public class MovementEffectController : MonoBehaviour
{
    [Header("Ustawienia efektu nieregularnego ruchu")]
    [SerializeField] private float minSpeedMultiplier = 0.6f;
    [SerializeField] private float maxSpeedMultiplier = 1.2f;
    
    private Player player;
    private float originalSpeed;

    private Coroutine irregularMovementRoutine;
    private Coroutine stopMovementRoutine;

    private void Awake()
    {
        player = Player.Instance;
        if (player == null)
        {
            Debug.LogError("Brak referencji do gracza (Player)!");
            return;
        }
        originalSpeed = player.GetModule<PlayerMovement>().moveSpeed;
    }

    // Publiczna metoda wywo³uj¹ca efekt nieregularnego ruchu.
    public void ExecuteIrregularMovementEffect(float duration)
    {
        if(irregularMovementRoutine != null)
            StopCoroutine(irregularMovementRoutine);
        irregularMovementRoutine = StartCoroutine(ApplyIrregularMovement(duration));
    }

    // Publiczna metoda wywo³uj¹ca efekt zatrzymania ruchu.
    public void ExecuteStopMovementEffect(float duration)
    {
        if(stopMovementRoutine != null)
            StopCoroutine(stopMovementRoutine);
        stopMovementRoutine = StartCoroutine(ApplyStopMovement(duration));
    }

    private IEnumerator ApplyIrregularMovement(float duration)
    {
        float randomMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        player.GetModule<PlayerMovement>().moveSpeed = originalSpeed * randomMultiplier;
        yield return new WaitForSeconds(duration);
        player.GetModule<PlayerMovement>().moveSpeed = originalSpeed;
    }

    private IEnumerator ApplyStopMovement(float duration)
    {
        player.GetModule<PlayerMovement>().moveSpeed = 0f;
        yield return new WaitForSeconds(duration);
        player.GetModule<PlayerMovement>().moveSpeed = originalSpeed;
    }
}