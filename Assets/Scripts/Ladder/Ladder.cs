using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class Ladder : MonoBehaviour, IInteractable
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
    public Vector3 topExitLocalOffset = new(0f, 0f, 0f);

    [Header("Custom Bottom Exit Settings")]
    [Tooltip("Local space offset (relative to the ladder transform) from the lower climbing point for the bottom exit.")]
    public Vector3 bottomExitLocalOffset = new(0f, 0f, 0f);

    [Header("Climbing Settings")]
    [Tooltip("General speed for climbing up and down the ladder.")]
    public float climbSpeed = 3f;
    
    [Header("Debug Settings")]
    [Tooltip("Player visualization.")]
    public bool showPlayerCollider;
    public Color playerColliderColor = Color.cyan;

    #endregion

    private Coroutine _movementCoroutine;
    private bool _isAlignedToLadder;
    private bool _isDescending;
    private bool _isAscending;

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

        var position = transform.position;
        Vector3 basePos = position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, position.y + upperClimbPointOffset, basePos.z);
        
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
        if (_player == null || _player.state != Player.State.Climbing || !_isAlignedToLadder) return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            ForceExitLadder();
            return;
        }

        var position = transform.position;
        Vector3 basePos = position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, position.y + upperClimbPointOffset, basePos.z);
            
        if (_player.Input.MoveInput.y > 0)
        {
            if (_player.transform.position.y < upperPoint.y - 0.05f)
            {
                if (!_isAscending)
                    StartCoroutine(SmoothAscent());
            }
            else
            {
                AttemptExit(ExitType.Top);
            }
        }
        else if (_player.Input.MoveInput.y < 0)
        {
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

    private void AttemptExit(ExitType exitType)
    {
        float exitThreshold = 0.15f;
        Vector3 exitTarget;
        
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);

        if (exitType == ExitType.Top)
        {
            exitTarget = upperPoint + transform.rotation * topExitLocalOffset;
        }
        else
        {
            exitTarget = lowerPoint + transform.rotation * bottomExitLocalOffset;
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

    private IEnumerator SmoothAscent()
    {
        _isAscending = true;
        Vector3 basePos = transform.position + transform.rotation * new Vector3(0, 0, ladderOffset);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);

        while (_player != null && _player.Input.MoveInput.y > 0 && _player.transform.position.y < upperPoint.y - 0.05f)
        {
            Vector3 currentPos = _player.transform.position;
            float newY = currentPos.y + climbSpeed * Time.deltaTime;
            newY = Mathf.Min(newY, upperPoint.y);
            _player.transform.position = new Vector3(basePos.x, newY, basePos.z);
            yield return null;
        }
        _isAscending = false;
    }

    private void ExitLadder(Vector3 exitPosition, float exitThreshold)
    {
        if (_movementCoroutine != null) 
            _player.ToggleInput();
        StopCoroutine(_movementCoroutine);
        _movementCoroutine = StartCoroutine(SmoothExit(exitPosition, exitThreshold));
    }

    private IEnumerator SmoothExit(Vector3 targetPosition, float threshold)
    {
        _isAlignedToLadder = false;
        _player.state = Player.State.Walking;

        while (Vector3.Distance(_player.transform.position, targetPosition) > threshold)
        {
            _player.transform.position = Vector3.Lerp(_player.transform.position, targetPosition, alignSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        _player.ToggleInput();
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
        var rotation = transform.rotation;
        Vector3 basePos = transform.position + rotation * new Vector3(0, 0, ladderOffset);
        Vector3 lowerPoint = new Vector3(basePos.x, transform.position.y + lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, transform.position.y + upperClimbPointOffset, basePos.z);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lowerPoint, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(upperPoint, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(lowerPoint, upperPoint);
        
        Vector3 lowerExitPoint = lowerPoint + rotation * bottomExitLocalOffset;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(lowerPoint, lowerExitPoint);
        Gizmos.DrawSphere(lowerExitPoint, 0.1f);
        
        Vector3 upperExitPoint = upperPoint + rotation * topExitLocalOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(upperPoint, upperExitPoint);
        Gizmos.DrawSphere(upperExitPoint, 0.1f);
        
        if (showPlayerCollider && _player != null)
        {
            CharacterController controller = _player.GetComponent<CharacterController>();
            if (controller != null)
            {
                Vector3 colliderSize = new Vector3(controller.radius * 2f, controller.height, controller.radius * 2f);
                Gizmos.color = playerColliderColor;
                Gizmos.DrawWireCube(lowerExitPoint, colliderSize);
                Gizmos.DrawWireCube(upperExitPoint, colliderSize);
            }
        }
    }

    #endregion
}
