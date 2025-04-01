using System.Collections;
using UnityEngine;

public class LadderClimb : MonoBehaviour, IInteractable
{
    private Player player;

    #region Designer Settings

    public enum ExitType { Top, Bottom }

    [Header("Raycast Settings")]
    [Tooltip("Distance for the bottom exit.")]
    public float bottomExitDistance = 0.5f;
    [Tooltip("Angle for the top exit.")]
    public float topExitAngle = 15f;
    [Tooltip("Distance for the top exit.")]
    public float topExitDistance = 2f;

    [Header("Alignment Settings")]
    [Tooltip("Speed at which the player aligns to the ladder.")]
    public float alignSpeed = 5f;
    [Tooltip("Offset from the ladder on the Z axis.")]
    public float ladderOffset = 1f;

    [Header("Climb Range Settings")]
    [Tooltip("Relative height offset for starting the climb (from ladder base).")]
    public float climbStartOffset = 0f;
    [Tooltip("Relative height offset for ending the climb (from ladder base).")]
    public float climbEndOffset = 2f;

    [Header("Climbing Settings")]
    [Tooltip("General speed for climbing up and down the ladder.")]
    public float climbSpeed = 3f;

    #endregion

    private Coroutine movementCoroutine;
    private bool isAlignedToLadder = false;
    private bool isDescending = false;

    #region IInteractable Implementation

    public void Interact(Player interactingPlayer)
    {
        if (interactingPlayer.state == Player.State.Climbing || isAlignedToLadder)
            return;

        player = interactingPlayer;
        if (player == null) return;

        player.ToggleInput();
        player.ToggleGravity();

        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        Vector3 startPos = new Vector3(basePos.x, transform.position.y + climbStartOffset, basePos.z);
        Vector3 endPos = new Vector3(basePos.x, transform.position.y + climbEndOffset, basePos.z);

        float midY = (startPos.y + endPos.y) / 2f;
        Vector3 targetPos = player.transform.position.y >= midY ? endPos : startPos;

        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(AlignPlayerToLadder(targetPos, () =>
        {
            isAlignedToLadder = true;
            player.state = Player.State.Climbing;
        }));
    }

    #endregion

    #region Climbing & Alignment

    private IEnumerator AlignPlayerToLadder(Vector3 targetPosition, System.Action onComplete)
    {
        // Target rotation faces opposite to the ladder's forward direction.
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);

        while (Vector3.Distance(player.transform.position, targetPosition) > 0.05f ||
               Quaternion.Angle(player.transform.rotation, targetRotation) > 1f)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, alignSpeed * Time.deltaTime);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
            yield return null;
        }

        player.transform.position = targetPosition;
        player.transform.rotation = targetRotation;
        yield return new WaitForSeconds(0.15f);
        player.ToggleInput();
        onComplete?.Invoke();
    }

    #endregion

    #region Update & Unified Exit Handling

    private void Update()
    {
        if (player != null && player.state == Player.State.Climbing && isAlignedToLadder)
        {
            // Forced exit safeguard: press Escape to exit ladder immediately.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ForceExitLadder();
                return;
            }

            // When pressing up, attempt to exit at the top.
            if (player.Input.MoveInput.y > 0)
            {
                AttemptExit(ExitType.Top);
            }
            // When pressing down, either smoothly descend or exit at the bottom.
            else if (player.Input.MoveInput.y < 0)
            {
                float minHeight = transform.position.y + climbStartOffset;
                // Smoothly move down if above the bottom threshold.
                if (player.transform.position.y > minHeight + 0.05f)
                {
                    if (!isDescending)
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
            exitOrigin = new Vector3(basePos.x, transform.position.y + climbEndOffset, basePos.z);
            // Calculate exit direction using the top exit angle.
            Vector3 alignedForward = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0) * Vector3.forward;
            exitDirection = Quaternion.Euler(topExitAngle, 0, 0) * alignedForward;
            rayDistance = topExitDistance;

            // Check that the path is clear.
            Ray exitRay = new Ray(player.transform.position, exitDirection);
            Debug.DrawRay(player.transform.position, exitDirection, Color.red);
            if (Physics.Raycast(exitRay, rayDistance))
                return;
        }
        else // Bottom exit
        {
            exitOrigin = new Vector3(basePos.x, transform.position.y + climbStartOffset, basePos.z);
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
        isDescending = true;
        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        float bottomY = transform.position.y + climbStartOffset;

        // Continuously move downward while input is held and not yet at the bottom threshold.
        while (player != null && player.Input.MoveInput.y < 0 && player.transform.position.y > bottomY + 0.05f)
        {
            Vector3 currentPos = player.transform.position;
            float newY = currentPos.y - climbSpeed * Time.deltaTime;
            newY = Mathf.Max(newY, bottomY);
            player.transform.position = new Vector3(basePos.x, newY, basePos.z);
            yield return null;
        }
        isDescending = false;
    }

    /// <summary>
    /// Initiates the smooth exit process with a given exit threshold.
    /// </summary>
    private void ExitLadder(Vector3 exitPosition, float exitThreshold)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(SmoothExit(exitPosition, exitThreshold));
    }

    /// <summary>
    /// Smoothly moves the player off the ladder.
    /// The coroutine ends once the player is within the given threshold.
    /// </summary>
    private IEnumerator SmoothExit(Vector3 targetPosition, float threshold)
    {
        isAlignedToLadder = false;
        player.state = Player.State.Walking;
        
        while (Vector3.Distance(player.transform.position, targetPosition) > threshold)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, climbSpeed * Time.deltaTime);
            yield return null;
        }
        // Do not force exact alignment; just allow exit when close enough.
        yield return new WaitForSeconds(1f);
        player.SetGravityEnabled(true);
    }

    #endregion

    #region Forced Exit

    /// <summary>
    /// Forcefully exits the ladder from the player's current position.
    /// This is a safeguard to ensure the player can always exit the ladder.
    /// </summary>
    private void ForceExitLadder()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        isAlignedToLadder = false;
        player.state = Player.State.Walking;
        player.ToggleInput();
        player.SetGravityEnabled(true);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        Vector3 basePos = transform.position + new Vector3(0, 0, ladderOffset);
        Vector3 startPos = new Vector3(basePos.x, transform.position.y + climbStartOffset, basePos.z);
        Vector3 endPos = new Vector3(basePos.x, transform.position.y + climbEndOffset, basePos.z);

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
