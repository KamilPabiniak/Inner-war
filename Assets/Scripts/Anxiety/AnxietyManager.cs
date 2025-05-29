using System;
using System.Collections;
using System.Collections.Generic;
using Anxiety.Controllers;
using UnityEngine;

namespace Anxiety
{
    public class AnxietyManager : MonoBehaviour
    {
        public static AnxietyManager Instance { get; private set; }
        [Range(0, 100)] public float FearLevel { get; private set; }

        [Header("DEBUG ONLY")] 
        public string PassiveFearLevelText;
        public string ActiveFearLevelText;
        
        [Space, Header("SETTINGS")] 
        public float increaseFearValueOnSpotted;
        public float increaseFearValueOnAttack;
        public float increaseFearValueOnActiveLevel;

        [Header("Controllers")] 
        public AudioEffectsController audioEffectsController;
        public MovementEffectController movementEffectController;
        public PostProcessingController postProcessingController;

        [Header("Passive fear profile levels")] 
        public FearLevelProfile passiveLevel0;
        public FearLevelProfile passiveLevel1;
        public FearLevelProfile passiveLevel2;
        public FearLevelProfile passiveLevel3;
        public FearLevelProfile passiveLevel4;
        public FearLevelProfile passiveLevel5;
        
        [Header("Active fear profile levels")] 
        public FearLevelProfile activeLevel0;
        public FearLevelProfile activeLevel1;
        public FearLevelProfile activeLevel2;
        public FearLevelProfile activeLevel3;
        public FearLevelProfile activeLevel4;
        public FearLevelProfile activeLevel5;
        public FearLevelProfile activeLevel6;
        public FearLevelProfile activeLevel7;
        
        private FearLevelProfile _currentProfile;
        private bool _isPlayerAlive = true;
        private readonly SortedSet<int> _activeLevels = new();
        private Coroutine _passiveLoopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnEnable()
        {
            GameEvents.onPlayerDied += OnPlayerDied;
            GameEvents.onPlayerRespawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            GameEvents.onPlayerDied -= OnPlayerDied;
            GameEvents.onPlayerRespawned -= OnPlayerRespawned;
        }

        private void Start()
        {
            InitializeAllEffects();
            UpdateProfile();
        }

        private void Update()
        {
            PassiveFearLevelText = FearLevel.ToString("F1");
            ActiveFearLevelText = _activeLevels.Count > 0 ? _activeLevels.Max.ToString("F0") : "0";
        }

        private void InitializeAllEffects()
        {
            // Init passive and active effect flags
            InitProfileEffects(passiveLevel0, true);
            InitProfileEffects(passiveLevel1, true);
            InitProfileEffects(passiveLevel2, true);
            InitProfileEffects(passiveLevel3, true);
            InitProfileEffects(passiveLevel4, true);
            InitProfileEffects(passiveLevel5, true);

            InitProfileEffects(activeLevel0, false);
            InitProfileEffects(activeLevel1, false);
            InitProfileEffects(activeLevel2, false);
            InitProfileEffects(activeLevel3, false);
            InitProfileEffects(activeLevel4, false);
            InitProfileEffects(activeLevel5, false);
            InitProfileEffects(activeLevel6, false);
            InitProfileEffects(activeLevel7, false);
        }

        private void InitProfileEffects(FearLevelProfile profile, bool isPassive)
        {
            if (profile == null) return;
            foreach (var effect in profile.effects)
                effect.Init(isPassive);
        }

        private void UpdateProfile()
        {
            if (!_isPlayerAlive) return;

            if (_activeLevels.Count == 0)
            {
                ApplyPassiveProfile();
            }
            else
            {
                int top = _activeLevels.Max;
                if (top <= 0)
                {
                    // treat 0 as clear
                    _activeLevels.Remove(0);
                    ApplyPassiveProfile();
                }
                else
                {
                    ApplyActiveProfile();
                }
            }
        }

        private void ApplyPassiveProfile()
        {
            int level = DeterminePassiveFearLevel();
            Debug.Log($"[Anxiety] Applying PASSIVE profile level {level}");

            var profile = level switch
            {
                0 => passiveLevel0,
                1 => passiveLevel1,
                2 => passiveLevel2,
                3 => passiveLevel3,
                4 => passiveLevel4,
                5 => passiveLevel5,
                _ => passiveLevel0
            };

            ApplyProfile(profile, true);
        }

        private void ApplyActiveProfile()
        {
            int top = _activeLevels.Max;
            Debug.Log($"[Anxiety] Applying ACTIVE profile level {top}");

            var profile = top switch
            {
                1 => activeLevel1,
                2 => activeLevel2,
                3 => activeLevel3,
                4 => activeLevel4,
                5 => activeLevel5,
                6 => activeLevel6,
                7 => activeLevel7,
                _ => activeLevel0
            };

            ApplyProfile(profile, false);
            if (top > 0 && increaseFearValueOnActiveLevel > 0)
                AddFear(increaseFearValueOnActiveLevel);
        }

        private void ApplyProfile(FearLevelProfile profile, bool isPassive)
        {
            if (_currentProfile == profile) return;

            Debug.Log($"[Anxiety] Switching profile to: {profile?.name} (Passive: {isPassive})");

            StopPassiveLoop();
            EndCurrentEffects();
            _currentProfile = profile;

            if (!_isPlayerAlive || _currentProfile == null) return;

            // trigger immediately
            TriggerEffectsBatch(_currentProfile);

            if (isPassive)
                _passiveLoopCoroutine = StartCoroutine(PassiveEffectsLoop());
        }

        private IEnumerator PassiveEffectsLoop()
        {
            while (_activeLevels.Count == 0 && _isPlayerAlive && _currentProfile != null)
            {
                yield return new WaitForSeconds(0.1f);
                TriggerEffectsBatch(_currentProfile);
            }
        }

        private void TriggerEffectsBatch(FearLevelProfile profile)
        {
            if (profile == null) return;
            foreach (var e in profile.effects)
            {
                e.SetBlocked(false);
                float interval = UnityEngine.Random.Range(profile.minInterval, profile.maxInterval);
                float duration = UnityEngine.Random.Range(profile.minDuration, profile.maxDuration);
                e.SetTiming(interval, duration);
                e.TriggerEffect();
            }
        }

        private void EndCurrentEffects()
        {
            if (_currentProfile == null) return;
            foreach (var e in _currentProfile.effects)
                e.ForceEndEffect();
        }

        public void AddFear(float amount)
        {
            FearLevel = Mathf.Clamp(FearLevel + amount, 0f, 100f);
            if (_activeLevels.Count == 0)
                ApplyPassiveProfile();
        }

        public int DeterminePassiveFearLevel() => FearLevel switch
        {
            <= 20 => 0,
            <= 40 => 1,
            <= 60 => 2,
            <= 80 => 3,
            _ => FearLevel < 100 ? 4 : 5
        };

        public void TriggerActiveContinuous(int level)
        {
            if (level == 0)
            {
                ClearAllActiveLevels();
                return;
            }
            if (level < 1 || level > 4) throw new ArgumentOutOfRangeException();
            _activeLevels.Add(level);
            UpdateProfile();
        }

        public void TriggerActiveTimed(int level, float duration)
        {
            if (level < 5 || level > 7) throw new ArgumentOutOfRangeException();
            _activeLevels.Add(level);
            UpdateProfile();
            TimerManager.Schedule(() => {
                _activeLevels.Remove(level);
                UpdateProfile();
            }, duration);
        }

        public void ClearActive(int level)
        {
            if (_activeLevels.Remove(level))
                UpdateProfile();
        }

        public void ClearAllActiveLevels()
        {
            if (_activeLevels.Count > 0)
            {
                Debug.Log("[Anxiety] Clearing ALL active levels");
                _activeLevels.Clear();
                UpdateProfile();
            }
        }

        private void StopPassiveLoop()
        {
            if (_passiveLoopCoroutine != null)
            {
                StopCoroutine(_passiveLoopCoroutine);
                _passiveLoopCoroutine = null;
            }
        }

        private void OnPlayerDied()
        {
            _isPlayerAlive = false;
            StopPassiveLoop();
            EndCurrentEffects();
            ClearAllActiveLevels();
        }

        private void OnPlayerRespawned()
        {
            _isPlayerAlive = true;
            UpdateProfile();
        }

        [ContextMenu("Give 20 passive fear")]
        public void GivePassiveFear() => AddFear(20f);
    }
}