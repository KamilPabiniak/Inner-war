using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Irregular Movement Effect", fileName = "NewIrregularMovementEffect")]
    public class IrregularMovementEffect : BaseFearEffect
    {
        [Header("Irregular Movement Effect")]
        [SerializeField] private float minSpeedMultiplier = 0.6f;
        [SerializeField] private float maxSpeedMultiplier = 1.2f;
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.movementEffectController.ExecuteIrregularMovementEffect(currentDuration, minSpeedMultiplier, maxSpeedMultiplier);
        }
        
    }
}