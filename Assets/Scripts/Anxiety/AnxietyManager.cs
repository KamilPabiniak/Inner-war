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

        // Coroutine for looping passive effects
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
            ApplyPassiveProfile();
        }

        private void Update()
        {
            PassiveFearLevelText = FearLevel.ToString("F1");
            ActiveFearLevelText = _activeLevels.Count > 0 ? _activeLevels.Max.ToString("F0") : "0";
        }

        private void InitializeAllEffects()
        {
            var allProfiles = new[]
            {
                passiveLevel0, passiveLevel1, passiveLevel2,
                passiveLevel3, passiveLevel4, passiveLevel5,
                activeLevel0, activeLevel1, activeLevel2,
                activeLevel3, activeLevel4, activeLevel5,
                activeLevel6, activeLevel7
            };
            foreach (var profile in allProfiles)
            {
                if (profile == null) continue;
                bool isPassive = profile.name.StartsWith("Passive") || profile == passiveLevel0;
                foreach (var effect in profile.effects)
                {
                    effect.Init(isPassive);
                }
            }
        }

        private void ApplyPassiveProfile()
        {
            int level = DeterminePassiveFearLevel();
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
            if (_activeLevels.Count == 0)
            {
                ApplyPassiveProfile();
                return;
            }

            int top = _activeLevels.Max;
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
            AddActiveFear();
        }

        private void ApplyProfile(FearLevelProfile profile, bool passive)
        {
            StopPassiveLoop(); 

            if (_currentProfile == profile) return;

            if (_currentProfile != null)
            {
                foreach (var e in _currentProfile.effects)
                    e.ForceEndEffect();
            }

            _currentProfile = profile;
            if (!_isPlayerAlive || _currentProfile == null) return;

            if (passive)
            {
                _passiveLoopCoroutine = StartCoroutine(PassiveEffectsLoop());
            }
            else
            {
                TriggerEffectsBatch(_currentProfile);
            }
        }


        private IEnumerator PassiveEffectsLoop()
        {
            while (_activeLevels.Count == 0 && _isPlayerAlive && _currentProfile != null)
            {
                TriggerEffectsBatch(_currentProfile);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void TriggerEffectsBatch(FearLevelProfile profile)
        {
            foreach (var e in profile.effects)
            {
                e.SetBlocked(false);
                float interval = UnityEngine.Random.Range(profile.minInterval, profile.maxInterval);
                float duration = UnityEngine.Random.Range(profile.minDuration, profile.maxDuration);
                e.SetTiming(interval, duration);
                e.TriggerEffect();
            }
        }

        public void ChangeFear(float amount)
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
            if (level < 1 || level > 4) throw new ArgumentOutOfRangeException();
            ClearAllActiveLevels();
            _activeLevels.Add(level);
            ApplyActiveProfile();
        }

        public void TriggerActiveTimed(int level, float duration)
        {
            if (level < 5 || level > 7) throw new ArgumentOutOfRangeException();
            ClearAllActiveLevels();
            _activeLevels.Add(level);
            ApplyActiveProfile();
            TimerManager.Schedule(() =>
            {
                _activeLevels.Remove(level);
                ApplyActiveProfile();
            }, duration);
        }

        public void ClearActive(int level)
        {
            if (_activeLevels.Remove(level))
                ApplyActiveProfile();
        }

        private void ClearAllActiveLevels()
        {
            if (_activeLevels.Count > 0)
            {
                if (_currentProfile != null)
                    foreach (var e in _currentProfile.effects)
                        e.ForceEndEffect();
                _activeLevels.Clear();
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

        private void AddActiveFear()
        {
            if (increaseFearValueOnActiveLevel > 0)
                ChangeFear(increaseFearValueOnActiveLevel);
        }
        

        private void OnPlayerDied()
        {
            _isPlayerAlive = false;
            StopPassiveLoop();
            if (_currentProfile != null)
                foreach (var e in _currentProfile.effects)
                    e.ForceEndEffect();
        }

        private void OnPlayerRespawned()
        {
            _isPlayerAlive = true;
            if (_activeLevels.Count > 0)
                ApplyActiveProfile();
            else
                ApplyPassiveProfile();
        }
    }
}
