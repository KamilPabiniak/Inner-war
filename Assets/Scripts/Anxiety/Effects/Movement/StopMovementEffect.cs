using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Stop Movement Effect", fileName = "NewStopMovementEffect")]
    public class StopMovementEffect : BaseFearEffect
    {
        // Ustawienia dla efektu, w którym postaæ samoistnie zatrzymuje siê,
        // np. czas, przez który postaæ nie reaguje na sterowanie.
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