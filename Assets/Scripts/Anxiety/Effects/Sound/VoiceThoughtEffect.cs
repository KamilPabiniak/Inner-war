using System.Collections.Generic;
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

        [SerializeField] private List<AudioClip> voicesSounds;
        private int _lastClipIndex;
        
        protected override void ExecuteEffect()
        {
            int clipIndex = Random.Range(0, voicesSounds.Count);
            
            if (voicesSounds.Count > 1)
            {
                while (clipIndex == _lastClipIndex)
                {
                    clipIndex = Random.Range(0, voicesSounds.Count);
                }
            }
            
            _lastClipIndex = clipIndex;
            AnxietyManager.Instance.audioEffectsController.PlayVoices(voicesFadeInDuration, voicesTargetVolume, voicesSounds[clipIndex]);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.StopVoices(voicesFadeOutDuration);
        }
    }
}