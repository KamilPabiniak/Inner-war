using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Sound Suppression", fileName = "NewSoundSuppression")]
    public class SoundSuppression : BaseFearEffect
    {
        [Header("Cut off Freq")]
        [SerializeField] private float cutOffOn = 350f;
        [Tooltip("Time to set the full muffle effect. The effect works immediately but here you set the time how fast it will set up")]
        [SerializeField] private float transitionTimeApply = 3f;
        [SerializeField] private float transitionTimeCancel = 3f;
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.audioEffectsController.ApplyAudioSuppression(cutOffOn, transitionTimeApply);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.ResetAudioSuppression(transitionTimeCancel);
        }
    }
}