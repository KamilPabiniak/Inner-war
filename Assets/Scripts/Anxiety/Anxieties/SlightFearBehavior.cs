using UnityEngine;

public class SlightFearBehavior : IFearBehavior
{
    private AnxietyManager _manager;
    private float _blurTimer;
    private float _blurEffectDuration;
    private PostProcessingManager _postProcessingManager;

    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        _postProcessingManager = _manager.GetVolume();
        ResetTimers();
        Debug.Log("Entering Slight Fear Behavior");
    }

    public void UpdateEffects()
    {
        _blurTimer += Time.deltaTime;

        if (_blurTimer >= _blurEffectDuration)
        {
            TriggerBlurEffect();
            ResetTimers();
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Slight Fear Behavior");
    }

    private void ResetTimers()
    {
        _blurTimer = 0f;
        _blurEffectDuration = Random.Range(25f, 35f);
    }

    private void TriggerBlurEffect()
    {
        Debug.Log("Triggering blur effect (handled via post-processing)");
        // Post-processing changes are handled via PostProcessingManager.
    }
}