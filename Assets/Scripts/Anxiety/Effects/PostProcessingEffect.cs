using UnityEngine;
using UnityEngine.Rendering;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Post Processing Effect", fileName = "NewPostProcessingEffect")]
    public class PostProcessingEffect : BaseFearEffect
    {
        [Binder("Use only one PostProcessingEffect on FearLevelProfile" , colorHex:"#ffe554")]
        [Binder("Transition Settings", fontSize: 15, fontStyle: FontStyle.Bold, bottomSpace:2f)]
        [SerializeField] private float startTransitionDuration = 1f; 
        [SerializeField] private float endTransitionDuration = 1f; 
        [Header("Profiles")]
        [SerializeField] private VolumeProfile postProcessingEffect;
        
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.postProcessingController.TurnOnEffects(startTransitionDuration, postProcessingEffect);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.postProcessingController.TurnOffEffects(endTransitionDuration);
        }
    }
}