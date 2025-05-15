using System.Collections.Generic;
using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/FearResponse", fileName = "NewFearResponse")]
    public class FearResponse : BaseFearEffect
    {
        [Header("Monologue Settings")] 
        [SerializeField] private float fadeInDuration = 0.1f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        [Range(0f, 1f)]
        [SerializeField] private float targetVolume = 1f;
        
        [Tooltip("Clips to be played in sequence. Each once, then reset.")]
        [SerializeField] private List<AudioClip> monologueClips;

        private int _currentIndex = 0;
        

        protected override void ExecuteEffect()
        {
            if (AnxietyManager.Instance.audioEffectsController.IsMonologuePlaying())
                return;
            
            if (monologueClips == null || monologueClips.Count == 0)
            {
                Debug.LogWarning("Monologue: the list of clips is empty!");
                return;
            }
            
            AudioClip clipToPlay = monologueClips[_currentIndex];
            
            AnxietyManager.Instance.audioEffectsController.PlayMonologue(
                fadeInDuration, 
                targetVolume, 
                clipToPlay
            );
            
            _currentIndex = (_currentIndex + 1) % monologueClips.Count;
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.StopMonologue(fadeOutDuration);
        }
    }
}