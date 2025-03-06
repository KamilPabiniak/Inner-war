using UnityEngine;

namespace Anxiety.Effects
{
    public abstract class BaseFearEffect : ScriptableObject
    {
        [Header("Ustawienia czasu")]
        public float minInterval = 1f;
        public float maxInterval = 2f; 
        public float minDuration = 4f;
        public float maxDuration = 8f; 

        [Header("Opcje wywo³ywania")]
        public bool autoTrigger;

        private bool _isActive;

        public void TriggerEffect()
        {
            float delay = Random.Range(minInterval, maxInterval);
            TimerManager.Schedule(() =>
            {
                float duration = Random.Range(minDuration, maxDuration);
                _isActive = true;
                ExecuteEffect();
            
                TimerManager.Schedule(() =>
                {
                    EndEffect();
                    _isActive = false;
                }, duration);
            
                if(autoTrigger && AnxietyManager.Instance != null && AnxietyManager.Instance.CurrentProfileContains(this))
                {
                    TriggerEffect();
                }
            }, delay);
        }
    
        protected abstract void ExecuteEffect();
        protected abstract void EndEffect();
    
        public void ForceEndEffect()
        {
            if (_isActive)
            {
                EndEffect();
                _isActive = false;
            }
        }
    }
}