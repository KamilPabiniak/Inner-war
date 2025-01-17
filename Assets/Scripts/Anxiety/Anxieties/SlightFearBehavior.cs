using UnityEngine;

public class SlightFearBehavior : IFearBehavior
{
    private AnxietyManager _manager;
    private TimerHandle _blurTimerHandle;
    private TimerHandle _audioTimerHandle;

    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        Debug.Log("Entering Level 1 Fear Behavior");
        StartBlurEffect();
        StartAudioEffect();
    }

    public void Exit()
    {
        CancelBlurEffect();
        CancelAudioEffect();
        Debug.Log("Exiting Level 1 Fear Behavior");
    }

    private void StartBlurEffect()
    {
        CancelBlurEffect();
        float blurDelay = Random.Range(25f, 35f);
        _blurTimerHandle = TimerManager.Schedule(() =>
        {
            _manager.postProcEffect.ApplyEdgeBlur();
            float blurDuration = Random.Range(10f, 20f);
            _blurTimerHandle = TimerManager.Schedule(CancelBlurEffect, blurDuration);
        }, blurDelay);
    }

    private void CancelBlurEffect()
    {
        if (_blurTimerHandle != null)
        {
            TimerManager.Cancel(_blurTimerHandle);
            _blurTimerHandle = null;
        }
        _manager.postProcEffect.ResetEffects();
    }

    private void StartAudioEffect()
    {
        CancelAudioEffect();
        float audioDelay = Random.Range(20f, 40f);
        _audioTimerHandle = TimerManager.Schedule(() =>
        {
            _manager.audioEffect.ApplyAudioMuffle();
            float audioDuration = Random.Range(15f, 20f);
            _audioTimerHandle = TimerManager.Schedule(CancelAudioEffect, audioDuration);
        }, audioDelay);
    }

    private void CancelAudioEffect()
    {
        if (_audioTimerHandle != null)
        {
            TimerManager.Cancel(_audioTimerHandle);
            _audioTimerHandle = null;
        }
        _manager.audioEffect.ResetAudioEffects();
    }
}
