using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Crouch Effect", fileName = "NewCrouchEffect")]
    public class CrouchEffect : BaseFearEffect
    {
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.movementEffectController.ExecuteAutoCrouchEffect(currentDuration);
        }
    }
}