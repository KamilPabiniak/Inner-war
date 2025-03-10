using System.Collections;
using UnityEngine;

namespace Anxiety.Controllers
{
    public class MovementEffectController : MonoBehaviour
    {
        private float _originalSpeed;

        private Coroutine _irregularMovementRoutine;
        private Coroutine _stopMovementRoutine;
        private Coroutine _autoCrouchRoutine;

        private PlayerMovement _playerMovement;

        private void Awake()
        {
            _originalSpeed = Player.Instance.GetModule<PlayerMovement>().moveSpeed;
            _playerMovement = Player.Instance.GetModule<PlayerMovement>();
        }
    
        public void ExecuteIrregularMovementEffect(float duration, float minSpeedMultiplier, float maxSpeedMultiplier)
        {
            if (_irregularMovementRoutine != null)
                StopCoroutine(_irregularMovementRoutine);
            _irregularMovementRoutine = StartCoroutine(ApplyIrregularMovement(duration, minSpeedMultiplier, maxSpeedMultiplier));
        }
    
        public void ExecuteStopMovementEffect(float duration)
        {
            if (_stopMovementRoutine != null)
                StopCoroutine(_stopMovementRoutine);
            _stopMovementRoutine = StartCoroutine(ApplyStopMovement(duration));
        }
    
        public void ExecuteAutoCrouchEffect(float duration)
        {
            if (_autoCrouchRoutine != null)
                StopCoroutine(_autoCrouchRoutine);
            _autoCrouchRoutine = StartCoroutine(ApplyAutoCrouch(duration));
        }

        private IEnumerator ApplyIrregularMovement(float duration, float minSpeedMultiplier, float maxSpeedMultiplier)
        {
            float randomMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
            _playerMovement.moveSpeed = _originalSpeed * randomMultiplier;
            yield return new WaitForSeconds(duration);
            _playerMovement.moveSpeed = _originalSpeed;
        }

        private IEnumerator ApplyStopMovement(float duration)
        {
            _playerMovement.moveSpeed = 0f;
            yield return new WaitForSeconds(duration);
            _playerMovement.moveSpeed = _originalSpeed;
        }
    
        private IEnumerator ApplyAutoCrouch(float duration)
        {
            float originalHeight = Player.Instance.standingHeight;
            _playerMovement.ForceCrouch();
            Player.Instance.characterController.height = Player.Instance.crouchHeight;
            yield return new WaitForSeconds(duration);
        
            _playerMovement.DisableForceCrouch();
            Player.Instance.characterController.height = originalHeight;
        }
    }
}
