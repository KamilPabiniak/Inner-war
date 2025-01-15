using UnityEngine;

public class AnxietyManager : MonoBehaviour
{
    [Range(0, 100)]
    private float FearLevel { get; set;}
    private IFearBehavior _currentBehavior;
    public PostProcessingManager postProcessingManager;

    [SerializeField] private float passiveFearIncreaseInterval = 15f;
    [SerializeField] private float passiveFearIncreaseAmount = 1f;

    private float _passiveTimer;

    private void Start()
    {
        UpdateBehavior();
    }

    private void Update()
    {
        _passiveTimer += Time.deltaTime;

        if (_passiveTimer >= passiveFearIncreaseInterval)
        {
            IncreaseFear(passiveFearIncreaseAmount);
            _passiveTimer = 0f;
        }

        _currentBehavior?.UpdateEffects();
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

    private void UpdateBehavior()
    {
        IFearBehavior newBehavior = AnxietyBehaviorFactory.GetBehavior(FearLevel);
        
        if (_currentBehavior?.GetType() != newBehavior?.GetType())
        {
            _currentBehavior?.Exit();
            _currentBehavior = newBehavior; 
            _currentBehavior.Enter(this); 
        }
        
        postProcessingManager.UpdatePostProcessingProfile(FearLevel);
    }



    public PostProcessingManager GetVolume() => postProcessingManager;
}
