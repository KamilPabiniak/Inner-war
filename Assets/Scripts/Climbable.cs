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
    [Tooltip("Offset from the ladder on the Z axis.")]
    public float ladderOffset = 1f;

    [Header("Climb Range Settings")]
    [Tooltip("Relative height offset for ending the climb (from ladder base).")]
    public float climbTopEndOffset;
    [Tooltip("Angle for the top exit.")]
    public float topExitAngle = 15f;
    [Tooltip("Distance for the top exit.")]
    public float topExitDistance = 2f;
    [Tooltip("Relative height offset for starting the climb (from ladder base).")]
    public float climbBottomStartOffset = -7;
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
        if (_player == null) return;

        _player.ToggleInput();
        _player.ToggleGravity();

        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        Vector3 startPos = new Vector3(basePos.x, transform.position.y + climbBottomStartOffset, basePos.z);
        Vector3 endPos = new Vector3(basePos.x, transform.position.y + climbTopEndOffset, basePos.z);

        float midY = (startPos.y + endPos.y) / 2f;
        Vector3 targetPos = _player.transform.position.y >= midY ? endPos : startPos;

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
        // Target rotation faces opposite to the ladder's forward direction.
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

    #region Update & Unified Exit Handling

    private void Update()
    {
        if (_player != null && _player.state == Player.State.Climbing && _isAlignedToLadder)
        {
            // Forced exit safeguard: press Escape to exit ladder immediately.
            if (Input.GetKeyDown(KeyCode.C))
            {
                ForceExitLadder();
                return;
            }

            // When pressing up, attempt to exit at the top.
            if (_player.Input.MoveInput.y > 0)
            {
                AttemptExit(ExitType.Top);
            }
            // When pressing down, either smoothly descend or exit at the bottom.
            else if (_player.Input.MoveInput.y < 0)
            {
                float minHeight = transform.position.y + climbBottomStartOffset;
                // Smoothly move down if above the bottom threshold.
                if (_player.transform.position.y > minHeight + 0.05f)
                {
                    if (!_isDescending)
                        StartCoroutine(SmoothDescent());
                }
                else
                {
                    // Once at the threshold, exit immediately.
                    AttemptExit(ExitType.Bottom);
                }
            }
        }
    }

    /// <summary>
    /// Unified method to handle both top and bottom exits.
    /// For Top, exit occurs if there is no obstacle and the player is within a threshold distance.
    /// For Bottom, exit is triggered immediately.
    /// </summary>
    private void AttemptExit(ExitType exitType)
    {
        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        Vector3 exitOrigin;
        Vector3 exitDirection;
        float rayDistance;
        float exitThreshold = 0.7f; // A threshold to allow smooth exit.

        if (exitType == ExitType.Top)
        {
            exitOrigin = new Vector3(basePos.x, transform.position.y + climbTopEndOffset, basePos.z);
            // Calculate exit direction using the top exit angle.
            Vector3 alignedForward = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0) * Vector3.forward;
            exitDirection = Quaternion.Euler(topExitAngle, 0, 0) * alignedForward;
            rayDistance = topExitDistance;

            // Check that the path is clear.
            Ray exitRay = new Ray(_player.transform.position, exitDirection);
            Debug.DrawRay(_player.transform.position, exitDirection, Color.red);
            if (Physics.Raycast(exitRay, rayDistance))
                return;
        }
        else 
        {
            exitOrigin = new Vector3(basePos.x, transform.position.y + climbBottomStartOffset, basePos.z);
            exitDirection = Vector3.down;
            rayDistance = bottomExitDistance;
        }

        Vector3 exitTarget = exitOrigin + exitDirection * rayDistance;
        ExitLadder(exitTarget, exitThreshold);
    }

    #endregion

    #region Smooth Movement Coroutines

    /// <summary>
    /// Smoothly descends the player along the ladder as long as the down input is held.
    /// </summary>
    private IEnumerator SmoothDescent()
    {
        _isDescending = true;
        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        float bottomY = transform.position.y + climbBottomStartOffset;

        // Continuously move downward while input is held and not yet at the bottom threshold.
        while (_player != null && _player.Input.MoveInput.y < 0 && _player.transform.position.y > bottomY + 0.05f)
        {
            Vector3 currentPos = _player.transform.position;
            float newY = currentPos.y - climbSpeed * Time.deltaTime;
            newY = Mathf.Max(newY, bottomY);
            _player.transform.position = new Vector3(basePos.x, newY, basePos.z);
            yield return null;
        }
        _isDescending = false;
    }

    /// <summary>
    /// Initiates the smooth exit process with a given exit threshold.
    /// </summary>
    private void ExitLadder(Vector3 exitPosition, float exitThreshold)
    {
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        _movementCoroutine = StartCoroutine(SmoothExit(exitPosition, exitThreshold));
    }

    /// <summary>
    /// Smoothly moves the player off the ladder.
    /// The coroutine ends once the player is within the given threshold.
    /// </summary>
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

    /// <summary>
    /// Forcefully exits the ladder from the player's current position.
    /// This is a safeguard to ensure the player can always exit the ladder.
    /// </summary>
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
        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        Vector3 startPos = new Vector3(basePos.x, transform.position.y + climbBottomStartOffset, basePos.z);
        Vector3 endPos = new Vector3(basePos.x, transform.position.y + climbTopEndOffset, basePos.z);

        // Visualize the ladder's climb range.
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPos, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(endPos, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);

        // Visualize the bottom exit.
        Vector3 bottomExitPoint = startPos + Vector3.down * bottomExitDistance;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(startPos, bottomExitPoint);
        Gizmos.DrawSphere(bottomExitPoint, 0.1f);

        // Visualize the top exit.
        Vector3 alignedForward = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0) * Vector3.forward;
        Vector3 topExitDirection = Quaternion.Euler(topExitAngle, 0, 0) * alignedForward;
        Vector3 topExitPoint = endPos + topExitDirection * topExitDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(endPos, topExitPoint);
        Gizmos.DrawSphere(topExitPoint, 0.1f);
    }

    #endregion
}
