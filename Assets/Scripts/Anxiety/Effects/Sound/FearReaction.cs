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
        private bool _playerDead = false;

        protected override void InitInside()
        {
            base.InitInside();
            _playerMonologue = Player.Instance.GetModule<PlayerMonologue>();
            _anxietyResponseCondition = Player.Instance.GetModule<AnxietyResponseCondition>();
            
            _playerMonologue.OnMonologueStarted += Interrupt;
            _anxietyResponseCondition.OnConditionAudioStarted += Interrupt;
            
            GameEvents.onPlayerDied += () => _playerDead = true;
            GameEvents.onPlayerRespawned += () => _playerDead = false;
        }

        protected override void ExecuteEffect()
        {
            if (_playerDead)
            {
                EndEffect();
                return;
            }
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
            if (_playerMonologue)
                _playerMonologue.OnMonologueStarted -= Interrupt;
            if (_anxietyResponseCondition)
                _anxietyResponseCondition.OnConditionAudioStarted -= Interrupt;
        }

        private void Interrupt()
        {
            EndEffect();
            ForceEndEffect();
        }
    }
}