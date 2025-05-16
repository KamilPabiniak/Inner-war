using System;
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
        [SerializeField] private List<AudioClip> fearResponseClips;

        private int _currentIndex = 0;
        private PlayerMonologue _playerMonologue;
        private AnxietyResponseCondition _anxietyResponseCondition;
        
        public override void Init()
        {
            base.Init();
            _playerMonologue = Player.Instance.GetModule<PlayerMonologue>();
            _anxietyResponseCondition = Player.Instance.GetModule<AnxietyResponseCondition>();
            
            _playerMonologue.OnMonologueStarted += Interrupt;
            _anxietyResponseCondition.OnConditionAudioStarted += Interrupt;
        }

        protected override void ExecuteEffect()
        {
            if (AnxietyManager.Instance.audioEffectsController.IsFearReactionPlaying()) return;
            if (_playerMonologue.isPlaying || _anxietyResponseCondition.IsAudioPlaying) { EndEffect(); return; }
            
            if (fearResponseClips == null || fearResponseClips.Count == 0)
            {
                Debug.LogWarning("Reactions: the list of clips is empty!");
                return;
            }
            
            AudioClip clipToPlay = fearResponseClips[_currentIndex];
            
            AnxietyManager.Instance.audioEffectsController.PlayFearReaction(
                fadeInDuration, 
                targetVolume, 
                clipToPlay
            );
            
            _currentIndex = (_currentIndex + 1) % fearResponseClips.Count;
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.StopFearReaction(fadeOutDuration);
        }

        private void OnDisable()
        {
            _playerMonologue.OnMonologueStarted -= Interrupt;
            _anxietyResponseCondition.OnConditionAudioStarted -= Interrupt;
        }

        private void Interrupt()
        {
            EndEffect();
            ForceEndEffect();
        }
    }
}