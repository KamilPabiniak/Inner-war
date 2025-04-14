using System.Collections;
using UnityEngine;

public class LadderClimb : MonoBehaviour, IInteractable
{
    private Player _player;

    #region Designer Settings

    private enum ExitType { Top, Bottom }

    [Header("Alignment Settings")]
    [Tooltip("Speed at which the player aligns to the ladder.")]
    public float alignSpeed = 15f;
    [Tooltip("Offset from the ladder on the local Z axis used as base position.")]
    public float ladderOffset = 1f;

    [Header("Climb Track Settings")]
    [Tooltip("Local space vertical offset for the lower point of the climbing track.")]
    public float lowerClimbPointOffset = -7f;
    [Tooltip("Local space vertical offset for the upper point of the climbing track.")]
    public float upperClimbPointOffset = 5f;

    [Header("Custom Top Exit Settings")]
    [Tooltip("Local space offset (relative to the ladder transform) from the upper climbing point for the top exit.")]
    public Vector3 topExitLocalOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("Local rotation offset (in Euler angles, relative to the ladder orientation) for the top exit direction.")]
    public Vector3 topExitLocalRotation = new Vector3(15f, 0f, 0f);
    [Tooltip("Distance for the top exit.")]
    public float topExitDistance = 2f;

    [Header("Custom Bottom Exit Settings")]
    [Tooltip("Local space offset (relative to the ladder transform) from the lower climbing point for the bottom exit.")]
    public Vector3 bottomExitLocalOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("Local rotation offset (in Euler angles, relative to the ladder orientation) for the bottom exit direction.")]
    public Vector3 bottomExitLocalRotation = new Vector3(0f, 0f, 0f);
    [Tooltip("Distance for the bottom exit.")]
    public float bottomExitDistance = 0.5f;

    [Header("Climbing Settings")]
    [Tooltip("General speed for climbing up and down the ladder.")]
    public float climbSpeed = 3f;

    #endregion

    private Coroutine _movementCoroutine;
    private bool _isAlignedToLadder;
    private bool _isDescending;

    #region IInteractable Implementation

    public void Interact(Player interactingPlayer)
    {
        if (interactingPlayer.state == Player.State.Climbing || _isAlignedToLadder)
            return;

        _player = interactingPlayer;
        if (_player == null)
            return;

        _player.ToggleInput();
        _player.ToggleGravity();

        // Obliczamy bazow¹ pozycjê drabiny z uwzglêdnieniem offsetu ladderOffset.
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        // Dolny i górny punkt œcie¿ki wyznaczamy na podstawie sta³ej pozycji Y drabiny oraz offsetów (w lokalnej przestrzeni)
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);

        // Wybieramy punkt docelowy w zale¿noœci od aktualnej wysokoœci gracza.
        float midY = (lowerPoint.y + upperPoint.y) / 2f;
        Vector3 targetPos = _player.transform.position.y >= midY ? upperPoint : lowerPoint;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(AlignPlayerToLadder(targetPos, () =>
        {
            _isAlignedToLadder = true;
            _player.state = Player.State.Climbing;
        }));
    }

    #endregion

    #region Climbing & Alignment

    private IEnumerator AlignPlayerToLadder(Vector3 targetPosition, System.Action onComplete)
    {
        // Ustawiamy gracza, aby by³ zwrócony twarz¹ do drabiny (odwrócony o 180° wzglêdem forward drabiny).
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);

        while (Vector3.Distance(_player.transform.position, targetPosition) > 0.05f ||
               Quaternion.Angle(_player.transform.rotation, targetRotation) > 1f)
        {
            _player.transform.position = Vector3.Lerp(_player.transform.position, targetPosition, alignSpeed * Time.deltaTime);
            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
            yield return null;
        }

        _player.transform.position = targetPosition;
        _player.transform.rotation = targetRotation;
        yield return new WaitForSeconds(0.15f);
        _player.ToggleInput();
        onComplete?.Invoke();
    }

    #endregion

    #region Update & Exit Handling

    private void Update()
    {
        if (_player != null && _player.state == Player.State.Climbing && _isAlignedToLadder)
        {
            // Umo¿liwienie wymuszonego wyjœcia z drabiny przy wciœniêciu Escape.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ForceExitLadder();
                return;
            }

            if (_player.Input.MoveInput.y > 0)
            {
                AttemptExit(ExitType.Top);
            }
            else if (_player.Input.MoveInput.y < 0)
            {
                // Sprawdzamy, czy gracz powinien zejœæ czy wyjœæ.
                Vector3 lowerPoint = new Vector3(transform.position.x + (transform.rotation * new Vector3(0, 0, ladderOffset)).x,
                                                 transform.position.y + lowerClimbPointOffset,
                                                 transform.position.z + (transform.rotation * new Vector3(0, 0, ladderOffset)).z);
                if (_player.transform.position.y > lowerPoint.y + 0.05f)
                {
                    if (!_isDescending)
                        StartCoroutine(SmoothDescent());
                }
                else
                {
                    AttemptExit(ExitType.Bottom);
                }
            }
        }
    }

    private void AttemptExit(ExitType exitType)
    {
        float exitThreshold = 0.7f; // Próg wykrywania osi¹gniêcia punktu wyjœcia.
        Vector3 exitTarget;

        // Obliczamy dolny i górny punkt toru.
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);

        if (exitType == ExitType.Top)
        {
            // Wyjœcie górne: od punktu upperPoint dodajemy offset wyjœcia (w przestrzeni lokalnej drabiny).
            Vector3 exitOrigin = upperPoint + transform.rotation * topExitLocalOffset;
            Vector3 exitDirection = transform.rotation * Quaternion.Euler(topExitLocalRotation) * Vector3.forward;
            exitTarget = exitOrigin + exitDirection * topExitDistance;

            // Opcjonalna detekcja kolizji: jeœli w kierunku wyjœcia jest przeszkoda, nie wychodzimy.
            Ray exitRay = new Ray(_player.transform.position, exitDirection);
            Debug.DrawRay(_player.transform.position, exitDirection, Color.red);
            if (Physics.Raycast(exitRay, topExitDistance))
                return;
        }
        else
        {
            // Wyjœcie dolne: od punktu lowerPoint dodajemy offset wyjœcia.
            Vector3 exitOrigin = lowerPoint + transform.rotation * bottomExitLocalOffset;
            Vector3 exitDirection = transform.rotation * Quaternion.Euler(bottomExitLocalRotation) * Vector3.forward;
            exitTarget = exitOrigin + exitDirection * bottomExitDistance;
        }

        ExitLadder(exitTarget, exitThreshold);
    }

    #endregion

    #region Smooth Movement Coroutines

    private IEnumerator SmoothDescent()
    {
        _isDescending = true;
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);

        while (_player != null && _player.Input.MoveInput.y < 0 && _player.transform.position.y > lowerPoint.y + 0.05f)
        {
            Vector3 currentPos = _player.transform.position;
            float newY = currentPos.y - climbSpeed * Time.deltaTime;
            newY = Mathf.Max(newY, lowerPoint.y);
            _player.transform.position = new Vector3(basePos.x, newY, basePos.z);
            yield return null;
        }
        _isDescending = false;
    }

    private void ExitLadder(Vector3 exitPosition, float exitThreshold)
    {
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        _movementCoroutine = StartCoroutine(SmoothExit(exitPosition, exitThreshold));
    }

    private IEnumerator SmoothExit(Vector3 targetPosition, float threshold)
    {
        _isAlignedToLadder = false;
        _player.state = Player.State.Walking;
        
        while (Vector3.Distance(_player.transform.position, targetPosition) > threshold)
        {
            _player.transform.position = Vector3.Lerp(_player.transform.position, targetPosition, climbSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        _player.SetGravityEnabled(true);
    }

    #endregion

    #region Forced Exit

    private void ForceExitLadder()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        _isAlignedToLadder = false;
        _player.state = Player.State.Walking;
        _player.ToggleInput();
        _player.SetGravityEnabled(true);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        // Obliczamy bazow¹ pozycjê drabiny.
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);

        // Wizualizacja toru (œcie¿ki wspinaczki).
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lowerPoint, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(upperPoint, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(lowerPoint, upperPoint);

        // Wizualizacja punktu dolnego wyjœcia.
        Vector3 lowerExitOrigin = lowerPoint + transform.rotation * bottomExitLocalOffset;
        Vector3 lowerExitDir = transform.rotation * Quaternion.Euler(bottomExitLocalRotation) * Vector3.forward;
        Vector3 lowerExitPoint = lowerExitOrigin + lowerExitDir * bottomExitDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(lowerPoint, lowerExitPoint);
        Gizmos.DrawSphere(lowerExitPoint, 0.1f);

        // Wizualizacja punktu górnego wyjœcia.
        Vector3 upperExitOrigin = upperPoint + transform.rotation * topExitLocalOffset;
        Vector3 upperExitDir = transform.rotation * Quaternion.Euler(topExitLocalRotation) * Vector3.forward;
        Vector3 upperExitPoint = upperExitOrigin + upperExitDir * topExitDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(upperPoint, upperExitPoint);
        Gizmos.DrawSphere(upperExitPoint, 0.1f);
    }

    #endregion
}
