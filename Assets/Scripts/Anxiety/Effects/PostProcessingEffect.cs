using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Post Processing Effect", fileName = "NewPostProcessingEffect")]
    public class PostProcessingEffect : BaseFearEffect
    {
        [Header("Transition Settings")]
        [SerializeField] private float startTransitionDuration = 1f; 
        [SerializeField] private float endTransitionDuration = 1f; 
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.postProcessingController.TurnOnEffects(startTransitionDuration);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.postProcessingController.TurnOffEffects(endTransitionDuration);
        }
    }
}