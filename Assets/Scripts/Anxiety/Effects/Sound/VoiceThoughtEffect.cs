using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Voice Thought Effect", fileName = "NewVoiceThoughtEffect")]
    public class VoiceThoughtEffect : BaseFearEffect
    {
        [Header("Voices Settings")]
        [SerializeField] private float voicesFadeInDuration = 1f;
        [SerializeField] private float voicesFadeOutDuration = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float voicesTargetVolume = 1f;

        [SerializeField] private AudioClip voicesSound;
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.audioEffectsController.PlayVoices(voicesFadeInDuration, voicesTargetVolume, voicesSound);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.StopVoices(voicesFadeOutDuration);
        }
    }
}