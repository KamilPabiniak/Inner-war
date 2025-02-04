using System.Collections.Generic;
using System.Globalization;
using Anxiety.Effects;
using UnityEngine;

namespace Anxiety
{
    public class AnxietyManager : MonoBehaviour
    {
        public static AnxietyManager Instance { get; private set; }
        [Range(0, 100)] public float FearLevel { get; set; }

        [Header("Passive Anxiety Increase Settings")]
        [SerializeField] private float passiveFearIncreaseInterval = 15f;
        [SerializeField] private float passiveFearIncreaseAmount = 1f;

        [Header("Effects Managers")]
        public PostProcessingManager postProcEffect;
        public AudioEffectsManager audioEffect;
        
        [Header("Debug (tylko do podgl¹du)")]
        [SerializeField] private string currentBehaviorName = "None";
        [SerializeField] private string currentFearLevelText = "None";
        

        [SerializeField] private List<EketySiema> Lvl1Effects = new List<EketySiema>(6);

        private void OnValidate()
        {
            while (Lvl1Effects.Count > 6)
            {
                Lvl1Effects.RemoveAt(Lvl1Effects.Count - 1);
            }
            
            while (Lvl1Effects.Count < 6)
            {
                Lvl1Effects.Add(default);
            }
        }
        


        private float _timer;
        private IFearBehavior _currentBehavior;
        private int _currentLevel;
        
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
            UpdateFearBehavior();
        }

        private void Update()
        {
            // Pasive fear increase over time
            _timer += Time.deltaTime;
            if (_timer >= passiveFearIncreaseInterval)
            {
                IncreaseFear(passiveFearIncreaseAmount);
                _timer = 0f;
            }

            currentFearLevelText = FearLevel.ToString(CultureInfo.CurrentCulture);
        }

        public void ExeciuteActiveLevel()
        {
            _currentBehavior?.Execute();
        }

        public void IncreaseFear(float amount)
        {
            FearLevel = Mathf.Clamp(FearLevel + amount, 0, 100);
            UpdateFearBehavior();
        }

        public void DecreaseFear(float amount)
        {
            FearLevel = Mathf.Clamp(FearLevel - amount, 0, 100);
            UpdateFearBehavior();
        }
        
        private int DetermineFearLevel()
        {
            if (FearLevel <= 20) return 0;
            if (FearLevel <= 40) return 1;
            if (FearLevel <= 60) return 2;
            if (FearLevel <= 80) return 3;
            if (FearLevel < 100) return 4;
            return 5;
        }

        private void UpdateFearBehavior()
        {
            int newLevel = DetermineFearLevel();
            if (newLevel != _currentLevel)
            {
                _currentBehavior?.Exit();
                _currentLevel = newLevel;
                _currentBehavior = CreateBehaviorForLevel(newLevel);
                _currentBehavior?.Enter(this);
                currentBehaviorName = _currentBehavior?.GetType().Name;
            }
        }
        
        private IFearBehavior CreateBehaviorForLevel(int level)
        {
            return level switch
            {
                0 => new AnxietyLevel_0(),
                1 => new AnxietyLevel_1(),
                2 => new AnxietyLevel_1(), //AnxietyLevel_2
                3 => new AnxietyLevel_1(), //AnxietyLevel_3
                4 => new AnxietyLevel_1(), // AnxietyLevel_4
                5 => new AnxietyLevel_1(), //AnxietyLevel_5
                _ => null
            };
        }
    }
}
