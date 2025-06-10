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

        private void Start()
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
    
        private IEnumerator ApplyAutoCrouch(float duration)
        {
            Player.Instance.ApplyCrouch(true);
            Player.Instance.GetModule<PlayerMovement>().ForceCrouch();
            yield return new WaitForSeconds(duration);
            Player.Instance.GetModule<PlayerMovement>().DisableForceCrouch();
            Player.Instance.ApplyCrouch(false);
        }
    }
}
