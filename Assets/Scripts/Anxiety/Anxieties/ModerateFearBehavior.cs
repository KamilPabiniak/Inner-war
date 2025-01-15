using UnityEngine;

public class ModerateFearBehavior : IFearBehavior
{
    private AnxietyManager _manager;
    private float _heartbeatTimer;
    private float _nextHeartbeatEffect;

    private float _screenShakeTimer;
    private float _nextScreenShakeEffect;

    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        ResetTimers();
    }

    public void UpdateEffects()
    {
        _heartbeatTimer += Time.deltaTime;
        _screenShakeTimer += Time.deltaTime;

        if (_heartbeatTimer >= _nextHeartbeatEffect)
        {
            TriggerHeartbeatSound();
            _heartbeatTimer = 0f;
            _nextHeartbeatEffect = Random.Range(10f, 45f);
        }

        if (_screenShakeTimer >= _nextScreenShakeEffect)
        {
            TriggerScreenShake();
            _screenShakeTimer = 0f;
            _nextScreenShakeEffect = Random.Range(25f, 35f);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting ModerateFearBehavior.");
    }

    private void ResetTimers()
    {
        _heartbeatTimer = 0f;
        _nextHeartbeatEffect = Random.Range(10f, 45f);

        _screenShakeTimer = 0f;
        _nextScreenShakeEffect = Random.Range(25f, 35f);
    }

    private void TriggerHeartbeatSound()
    {
        Debug.Log("Heartbeat sound triggered (handled via external audio system).");
        // Dodaj logikê wyzwalania efektu dŸwiêku bicia serca.
    }

    private void TriggerScreenShake()
    {
        Debug.Log("Screen shake triggered.");
        // Dodaj logikê wyzwalania lekkiego efektu trzêsienia ekranu (np. przez kamerê).
    }
}