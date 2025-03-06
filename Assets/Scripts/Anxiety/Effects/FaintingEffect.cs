using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Fainting Effect", fileName = "NewFaintingEffect")]
    public class FaintingEffect : BaseFearEffect
    {
        // Ustawienia dla efektu mdlenia, który koñczy rozgrywkê,
        // np. opóŸnienie przed "omdleniem" oraz animacja.
        protected override void ExecuteEffect()
        {
            throw new System.NotImplementedException();
        }

        protected override void EndEffect()
        {
            throw new System.NotImplementedException();
        }
    }
}