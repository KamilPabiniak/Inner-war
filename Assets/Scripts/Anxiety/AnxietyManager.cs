using System;
using UnityEngine;

public class AnxietyManager : MonoBehaviour
{
    [Range(0, 100)] public float FearLevel { get; private set; }

    [SerializeField] private float passiveFearIncreaseInterval = 15f;
    [SerializeField] private float passiveFearIncreaseAmount = 1f;

    [Header("Effect")]
    public PostProcessingManager postProcEffect;
    public AudioEffectsManager audioEffect;

    private float _timer;
    private IFearBehavior _currentBehavior;
    private float _lastFearLevelThreshold;

    private void Start()
    {
        UpdateBehavior();
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
    }

    public void IncreaseFear(float amount)
    {
        FearLevel = Mathf.Clamp(FearLevel + amount, 0, 100);
        UpdateBehavior();
    }

    public void DecreaseFear(float amount)
    {
        FearLevel = Mathf.Clamp(FearLevel - amount, 0, 100);
        UpdateBehavior();
    }

    private void UpdateBehavior(bool forceUpdate = false)
    {
        float newThreshold = AnxietyBehaviorFactory.GetThreshold(FearLevel);

        if (forceUpdate || newThreshold != _lastFearLevelThreshold)
        {
            IFearBehavior newBehavior = AnxietyBehaviorFactory.GetBehavior(newThreshold);
            postProcEffect.UpdatePostProcessingProfile(FearLevel);

            if (_currentBehavior != newBehavior)
            {
                _currentBehavior?.Exit();
                _currentBehavior = newBehavior;
                _currentBehavior?.Enter(this);
            }

            _lastFearLevelThreshold = newThreshold;
        }
    }
}
