using UnityEngine;

namespace Anxiety.Effects
{
    public abstract class BaseFearEffect : ScriptableObject
    {
        [Header("Base time set")]
        public float minInterval = 1f;
        public float maxInterval = 2f; 
        public float minDuration = 4f;
        public float maxDuration = 8f; 

        [Header("Call option")]
        public bool autoTrigger;
        public bool disableWhenTrigger;

        private bool _isActive;
        private bool _isBlocked;

        private void OnEnable()
        {
            _isBlocked = false;
            _isActive = false;
        }

        public void SetBlocked(bool blocked)
        {
            _isBlocked = blocked;
        }

        public void TriggerEffect()
        {
            if (_isBlocked)
                return;

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

                    if (disableWhenTrigger && AnxietyManager.Instance != null)
                    {
                        AnxietyManager.Instance.UnblockAutoTriggeredEffects();
                    }

                    if (autoTrigger && AnxietyManager.Instance != null && AnxietyManager.Instance.CurrentProfileContains(this))
                    {
                        TriggerEffect();
                    }
                }, duration);
            }, delay);
        }

        protected abstract void ExecuteEffect();
        protected abstract void EndEffect();

        public void ForceEndEffect()
        {
            if (!_isActive) return;
            EndEffect();
            _isActive = false;
        }
    }
}
