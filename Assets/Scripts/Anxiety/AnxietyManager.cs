using System.Linq;
using Anxiety.Controllers;
using Anxiety.Effects;
using UnityEngine;

namespace Anxiety
{
    public class AnxietyManager : MonoBehaviour
    {
        public static AnxietyManager Instance { get; private set; }
        [Range(0, 100)] public float FearLevel { get; private set; }

        [Header("DEBUG ONLY")]
        public string FearLevelText;

        [Header("Passive fear")]
        [SerializeField] private float passiveFearIncreaseInterval = 15f;
        [SerializeField] private float passiveFearIncreaseAmount = 1f;

        [Header("Controllers")] 
        public AudioEffectsController audioEffectsController;
        public MovementEffectController movementEffectController;
        public PostProcessingController postProcessingController;

        [Header("Fear profile levels")]
        public FearLevelProfile level0Profile;
        public FearLevelProfile level1Profile;
        public FearLevelProfile level2Profile;
        public FearLevelProfile level3Profile;
        public FearLevelProfile level4Profile;
        public FearLevelProfile level5Profile;
        
        private FearLevelProfile _currentProfile;
        private float _timer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            UpdateProfileForLevel(DetermineFearLevel());
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= passiveFearIncreaseInterval)
            {
                IncreaseFear(passiveFearIncreaseAmount);
                _timer = 0f;
            }

            FearLevelText = FearLevel.ToString();
        }

        public void IncreaseFear(float amount)
        {
            FearLevel = Mathf.Clamp(FearLevel + amount, 0, 100);
            int level = DetermineFearLevel();
            UpdateProfileForLevel(level);
        }

        public void DecreaseFear(float amount)
        {
            FearLevel = Mathf.Clamp(FearLevel - amount, 0, 100);
            int level = DetermineFearLevel();
            UpdateProfileForLevel(level);
        }

        public int DetermineFearLevel()
        {
            return FearLevel switch
            {
                <= 20 => 0,
                <= 40 => 1,
                <= 60 => 2,
                <= 80 => 3,
                _ => FearLevel < 100 ? 4 : 5
            };
        }

        private void UpdateProfileForLevel(int level)
        {
            _currentProfile = level switch
            {
                0 => level0Profile,
                1 => level1Profile,
                2 => level2Profile,
                3 => level3Profile,
                4 => level4Profile,
                5 => level5Profile,
                _ => null
            };
            if (_currentProfile == null) return;
            foreach (var effect in _currentProfile.effects.Where(effect => effect != null).Where(effect => effect.autoTrigger))
            {
                effect.TriggerEffect();
            }
        }

        [ContextMenu("TriggerEffects")]
        public void TriggerProfileEffects()
        {
            if (_currentProfile == null) return;

            bool shouldBlock = _currentProfile.effects.Any(e => e != null && e.disableWhenTrigger);
            if (shouldBlock && Instance != null)
            {
                BlockAutoTriggeredEffects();
            }
    
            foreach (var effect in _currentProfile.effects.Where(e => e != null))
            {
                effect.TriggerEffect();
            }
        }
        
        public bool CurrentProfileContains(BaseFearEffect effect)
        {
            return _currentProfile != null && _currentProfile.effects.Contains(effect);
        }

        private void BlockAutoTriggeredEffects()
        {
            if (_currentProfile == null) return;
            foreach (var effect in _currentProfile.effects.Where(e => e != null && e.autoTrigger))
            {
                effect.ForceEndEffect();
                effect.SetBlocked(true);
            }
        }

        public void UnblockAutoTriggeredEffects()
        {
            if (_currentProfile == null) return;
            foreach (var effect in _currentProfile.effects.Where(e => e != null && e.autoTrigger))
            {
                effect.SetBlocked(false);
            }
        }
    }
}
