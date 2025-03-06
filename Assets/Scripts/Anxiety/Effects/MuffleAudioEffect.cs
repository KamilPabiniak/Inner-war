using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Muffle Audio Effect", fileName = "NewMuffleAudioEffect")]
    public class MuffleAudioEffect : BaseFearEffect
    {
        protected override void ExecuteEffect()
        {
     
        }

        protected override void EndEffect()
        {
            throw new System.NotImplementedException();
        }
    }
}