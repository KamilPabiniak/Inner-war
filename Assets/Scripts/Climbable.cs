using System.Collections;
using UnityEngine;

public class Climbable : MonoBehaviour, IInteractable
{
    private Player _player;

    [Header("Raycast Settings")]
    public float bottomRaycastDistance = 0.5f;
    public float topRaycastAngle = 15f;
    public float topRaycastDistance = 2f;

    public float alignmentSpeed;
    public float offsetFromLadder = 1f; 
    private Coroutine climbCoroutine;
    private bool isClimbingAligned = false; 

    public void Interact(Player player)
    {
        if (player.state == Player.State.Climbing || isClimbingAligned) return;
        _player = player;
        if (_player == null) return;
        _player.ToggleInput();
        _player.ToggleGravity();

        if (climbCoroutine != null) StopCoroutine(climbCoroutine);
        climbCoroutine = StartCoroutine(AlignToLadderCoroutine(() =>
        {
            isClimbingAligned = true;
            _player.state = Player.State.Climbing;
        }));
    }
    
    private void Update()
    {
        if (_player != null && _player.state == Player.State.Climbing && isClimbingAligned)
        {
            HandleBottomExit();
            HandleTopExit();
        }
    }

    private IEnumerator AlignToLadderCoroutine(System.Action onComplete)
    {
        Vector3 targetPosition = transform.position + new Vector3(0, 0, offsetFromLadder);
        targetPosition.y = _player.transform.position.y;

        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);

        while (Vector3.Distance(_player.transform.position, targetPosition) > 0.05f ||
               Quaternion.Angle(_player.transform.rotation, targetRotation) > 1f)
        {
            _player.transform.position = Vector3.Lerp(_player.transform.position, targetPosition, alignmentSpeed * Time.deltaTime);
            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRotation, alignmentSpeed * Time.deltaTime);

            yield return null;
        }

        _player.transform.position = targetPosition;
        _player.transform.rotation = targetRotation;
        _player.ToggleInput();
        onComplete?.Invoke();
    }

    private void HandleBottomExit()
    {
        if (_player.Input.MoveInput.y >= 0) return;

        Ray ray = new Ray(_player.transform.position, Vector3.down);
        Debug.DrawRay(_player.transform.position, Vector3.down, Color.magenta);

        if (Physics.Raycast(ray, out RaycastHit hit, bottomRaycastDistance))
        {
            ExitLadder(Vector3.back); 
        }
    }

    private void HandleTopExit()
    {
        if (_player.Input.MoveInput.y <= 0) return;

        Vector3 direction = Quaternion.Euler(topRaycastAngle, 0, 0) * _player.transform.forward;
        Ray ray = new Ray(_player.transform.position, direction);
        Debug.DrawRay(_player.transform.position, direction, Color.red);

        if (!Physics.Raycast(ray, out RaycastHit hit, topRaycastDistance))
        {
            ExitLadder(Vector3.forward);
        }
    }

    private void ExitLadder(Vector3 exitDirection)
    {
        if (climbCoroutine != null) StopCoroutine(climbCoroutine);
        climbCoroutine = StartCoroutine(SmoothExitLadder(exitDirection));
    }

    private IEnumerator SmoothExitLadder(Vector3 exitDirection)
    {
        isClimbingAligned = false; 
        _player.state = Player.State.Walking;

        Vector3 targetPosition = _player.transform.position + exitDirection * 0.5f;

        _player.transform.position = targetPosition;
        yield return new WaitForSeconds(1f);
        _player.SetGravityEnabled(true);
    }
}
