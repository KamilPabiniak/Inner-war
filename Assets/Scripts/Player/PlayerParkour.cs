using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerParkour : PlayerModule
{
    [Header("Climbing Settings")]
    public LayerMask vaultLayer;
    public float climbSpeed = 3f;      
    public float vaultDistance = 1.5f;
    public float climbHeight = 2f;  
    public float playerRadius = 0.5f; 
    public float ledgeOffset = 0.1f;
    
    private PlayerInput _inputHandler;
    private Transform _cameraTransform;
    private bool _isClimbing = false;
    private bool _vaultRequested;
    
    private static readonly RaycastHit[] _raycastHits = new RaycastHit[2];

    protected override void OnInitialize()
    {
        _cameraTransform = Player.cameraTransform;
        _inputHandler = Player.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!_inputHandler.IsVaultPressed || _isClimbing) return;
        _vaultRequested = true;
        _inputHandler.ResetVaultRequest();
    }

    private void FixedUpdate()
    {
        if (_isClimbing || !_vaultRequested) return;
        _vaultRequested = false;
        TryVault();
    }
    
      private void TryVault()
       {
           if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward,
               out RaycastHit firstHit, vaultDistance, vaultLayer.value))
           {
               Vector3 climbStart = firstHit.point + _cameraTransform.forward * playerRadius + Vector3.up * (0.6f * climbHeight);
               
               if (Physics.Raycast(climbStart, Vector3.down, out RaycastHit secondHit,
                   climbHeight, vaultLayer.value))
               {
                   StartCoroutine(Climb(secondHit.point));
               }
           }
       }
    
    private IEnumerator Climb(Vector3 targetPosition)
    {
        _isClimbing = true;
        Player.characterController.enabled = false;

        Vector3 startPosition = transform.position;
        Vector3 finalPosition = new Vector3(targetPosition.x, targetPosition.y + ledgeOffset, targetPosition.z);
        float distance = Vector3.Distance(startPosition, finalPosition);
        float climbDuration = distance / climbSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < climbDuration)
        {
            transform.position = Vector3.Lerp(startPosition, finalPosition, elapsedTime / climbDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = finalPosition;
        Player.characterController.enabled = true;
        _isClimbing = false;
    }
}
