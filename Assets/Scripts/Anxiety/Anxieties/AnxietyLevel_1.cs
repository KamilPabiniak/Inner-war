using Anxiety;
using UnityEngine;

public class AnxietyLevel_1 : IFearBehavior
{
    private AnxietyManager _manager;
    
    private TimerHandle _blurTimerHandle;
    private TimerHandle _audioTimerHandle;

    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        Debug.Log("Entering Level 1 Anxiety Behavior");
    }

    public void Exit()
    {
        Debug.Log("Exiting Level 1 Anxiety Behavior");
    }

    public void Execute()
    {
        StartBlurEffect();
        StartAudioEffect();
    }

    private void StartBlurEffect()
    {
        if (_blurTimerHandle != null) return;
        CancelBlurEffect();
        float blurDelay = 0.1f;
        _blurTimerHandle = TimerManager.Schedule(() =>
        {
            _manager.postProcEffect.TurnOnEffects(2f);
            float blurDuration = 10f;
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
        _manager.postProcEffect.DisableEffects(3f);
    }

    private void StartAudioEffect()
    {
        if (_audioTimerHandle != null) return;
        CancelAudioEffect();
        float audioDelay = 0.1f;
        _audioTimerHandle = TimerManager.Schedule(() =>
        {
            _manager.audioEffect.ApplyAudioMuffle(2f);
            float audioDuration = 10f;
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
        _manager.audioEffect.ResetAudioEffects(3f);
    }
}