using UnityEngine;

namespace Anxiety.Effects
{
    public abstract class BaseFearEffect : ScriptableObject
    {
        private bool _isActive;
        private bool _isBlocked;
        private bool _isPassive;
        private bool _isScheduled;
        private float _currentInterval;
        protected float currentDuration;

        public void Init(bool isPassive)
        {
            _isPassive = isPassive;
            InitInside();
        }

        private void OnEnable()
        {
            _isBlocked = false;
            _isActive = false;
            _isScheduled = false;
        }

        public void SetBlocked(bool blocked) => _isBlocked = blocked;

        public void SetTiming(float interval, float duration)
        {
            _currentInterval = interval;
            currentDuration = duration;
        }

        public void TriggerEffect()
        {
            // Prevent scheduling or execution if already active, blocked, or pending
            if (_isBlocked || _isActive || _isScheduled)
                return;

            _isScheduled = true;

            if (_isPassive)
            {
                // Schedule the effect after the interval
                TimerManager.Schedule(() =>
                {
                    _isScheduled = false;

                    if (_isBlocked)
                        return;

                    _isActive = true;
                    ExecuteEffect();

                    // Schedule end of effect
                    TimerManager.Schedule(() =>
                    {
                        if (!_isBlocked)
                        {
                            EndEffect();
                        }
                        // Reset active flag to allow new triggers
                        _isActive = false;
                    }, currentDuration);
                }, _currentInterval);
            }
            else
            {
                // Immediate execution for active-only effects
                _isActive = true;
                ExecuteEffect();
            }
        }

        public void ForceEndEffect()
        {
            if (!_isActive && !_isScheduled) return;

            _isBlocked = true;
            _isScheduled = false;

            if (_isActive)
            {
                EndEffect();
                _isActive = false;
            }
        }

        protected virtual void InitInside() { }
        protected abstract void ExecuteEffect();
        protected virtual void EndEffect() { }
    }
}
