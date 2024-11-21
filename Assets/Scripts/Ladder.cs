using System.Collections;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [Header("Ladder Points")]
    public Transform bottomPoint;
    public Transform topPoint;
    public Transform bottomExitPoint;
    public Transform topExitPoint;
    public float climbSpeed = 3f;

    private bool isClimbing = false;
    private Player _player;
    private PlayerInput playerInput;
    private CharacterController characterController;

    public void Interact(Player player)
    {
        if (isClimbing) return;

        _player = player;
        playerInput = player.GetComponent<PlayerInput>();
        characterController = player.characterController;

        StartClimbing();
    }

    public bool IsClimbing() => isClimbing;
    
    private IEnumerator MoveToLocation(Vector3 targetPosition, Quaternion targetRotation)
    {
        float duration = 0.5f; 
        float elapsedTime = 0f;

        Vector3 startPosition = _player.transform.position;
        Quaternion startRotation = _player.transform.rotation;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            _player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            _player.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;

        _player.gravityEnabled = false;
        _player.DisableInput();
        
        Vector3 directionToLadder = (transform.position - _player.transform.position).normalized;
        directionToLadder.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToLadder);

     
        float distanceToBottom = Vector3.Distance(_player.transform.position, bottomPoint.position);
        float distanceToTop = Vector3.Distance(_player.transform.position, topPoint.position);

        Vector3 targetPosition = distanceToBottom < distanceToTop ? bottomPoint.position : topPoint.position;
        
        StartCoroutine(MoveToLocation(targetPosition, targetRotation));
    }


    private void Update()
    {
        if (!isClimbing) return;
        HandleClimbing();
    }

    private void HandleClimbing()
    {
        Vector2 moveInput = playerInput.MoveInput;
        float verticalInput = moveInput.y;

        if (verticalInput != 0)
        {
            Vector3 climbMovement = transform.up * verticalInput * climbSpeed * Time.deltaTime;
            characterController.Move(climbMovement);
        }
    }

    public void ExitLadder(bool isTop)
    {
        isClimbing = false;
        _player.gravityEnabled = true;

        Vector3 exitPosition = isTop ? topExitPoint.position : bottomExitPoint.position;
        Quaternion exitRotation = Quaternion.LookRotation(-transform.forward);
        StartCoroutine(MoveToLocation(exitPosition, exitRotation));

        _player.EnableInput();
    }

}
