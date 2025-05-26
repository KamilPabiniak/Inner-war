using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectsController : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    private AudioSource _currentFearReaction;
    private AudioSource _currentVoices;
    private AudioSource _currentHeart;

    #region Cutoff Frequency Transition Methods

    private Coroutine _audioTransitionCoroutine;
    
    public void ApplyAudioSuppression(float cutOffOn, float applyDuration)
    {
        if (_audioTransitionCoroutine != null)
            StopCoroutine(_audioTransitionCoroutine);
        _audioTransitionCoroutine = StartCoroutine(TransitionCutoff(cutOffOn, applyDuration));
    }
    
    public void ResetAudioSuppression(float applyDuration)
    {
        _audioTransitionCoroutine = StartCoroutine(TransitionCutoff(22000f, applyDuration));
    }
    
    private IEnumerator TransitionCutoff(float targetValue, float duration)
    {
        audioMixer.GetFloat("cutoffFreq", out float startFreq);
        audioMixer.GetFloat("cutoffMusic", out float startMusic);
        
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            float newFreq = Mathf.Lerp(startFreq, targetValue, smoothT);
            float newMusic = Mathf.Lerp(startMusic, targetValue, smoothT);
            
            audioMixer.SetFloat("cutoffFreq", newFreq);
            audioMixer.SetFloat("cutoffMusic", newMusic);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioMixer.SetFloat("cutoffFreq", targetValue);
        audioMixer.SetFloat("cutoffMusic", targetValue);
    }

    #endregion
    
    public bool IsFearReactionPlaying()
    {
        return _currentFearReaction != null && _currentFearReaction.isPlaying;
    }
    
    public void PlayFearReaction(float monologueFadeInDuration, float monologueTargetVolume, AudioClip monologueClip)
    {
        if (IsFearReactionPlaying())
            return;
        
        _currentFearReaction = SoundFXManager.Instance.Play2DSoundFXClipDestroyOn(
            monologueClip,
            transform,
            monologueTargetVolume,
            destroyTime: monologueClip.length,
            onLoop: false,
            audioMixerGroup: SoundFXManager.Instance.MonologueMixer);
        
        if (!_currentFearReaction) return;
        StartCoroutine(FadeIn(_currentFearReaction, monologueFadeInDuration, monologueTargetVolume));
    }
    
    public void StopFearReaction(float monologueFadeOutDuration)
    {
        if (_currentFearReaction != null)
        {
            StartCoroutine(FadeOutAndStop(_currentFearReaction, monologueFadeOutDuration, () =>
            {
                _currentFearReaction = null;
            }));
        }
    }
    
    public void PlayVoices(float voicesFadeInDuration, float voicesTargetVolume, AudioClip voicesClip)
    {
        _currentVoices = SoundFXManager.Instance.Play2DSoundFXClipDestroyOn(
            voicesClip,
            transform,
            voicesTargetVolume,
            destroyTime: voicesClip.length,
            onLoop: false,
            audioMixerGroup: SoundFXManager.Instance.VoicesMixer);
        
        if (!_currentVoices) return;
        StartCoroutine(FadeIn(_currentVoices, voicesFadeInDuration, voicesTargetVolume));
    }
    
    public void StopVoices(float voicesFadeOutDuration)
    {
        if (_currentVoices != null)
        {
            StartCoroutine(FadeOutAndStop(_currentVoices, voicesFadeOutDuration, () =>
            {
                _currentVoices = null;
            }));
        }
    }
    
    public void PlayHeart(float targetDrymixEchoOnValue, float heartFadeInDuration, float heartTargetVolume, AudioClip heartClip)
    {
        audioMixer.SetFloat("drymixEcho", 0f);

        float estimatedDuration = heartClip.length * 10f; 

        _currentHeart = SoundFXManager.Instance.Play2DSoundFXClipDestroyOn(
            heartClip,
            transform,
            heartTargetVolume,
            destroyTime: estimatedDuration,
            onLoop: true,
            audioMixerGroup: SoundFXManager.Instance.HeartMixer);

        if (!_currentHeart) return;
        StartCoroutine(FadeInHeart(_currentHeart, heartFadeInDuration, heartTargetVolume, targetDrymixEchoOnValue));
    }
    
    public void StopHeart(float heartFadeOutDuration)
    {
        if (_currentHeart != null)
        {
            StartCoroutine(FadeOutHeart(_currentHeart, heartFadeOutDuration, () =>
            {
                _currentHeart = null;
            }));
        }
    }
    
    private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        float startVolume = 0f;
        source.volume = 0f;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.SmoothStep(0f, 1f, t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume;
    }
    
    private IEnumerator FadeOutAndStop(AudioSource source, float duration, System.Action onComplete)
    {
        float startVolume = source.volume;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            source.volume = Mathf.Lerp(startVolume, 0f, Mathf.SmoothStep(0f, 1f, t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
        onComplete?.Invoke();
    }
    
    private IEnumerator FadeInHeart(AudioSource heartSource, float duration, float targetVolume, float targetDrymixEchoOnValue)
    {
        float startVolume = 0f;
        heartSource.volume = 0f;
        audioMixer.GetFloat("drymixEcho", out float startEcho);
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            if (heartSource)
            {
                heartSource.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.SmoothStep(0f, 1f, t));
                float newEcho = Mathf.Lerp(startEcho, targetDrymixEchoOnValue, Mathf.SmoothStep(0f, 1f, t));
                audioMixer.SetFloat("drymixEcho", newEcho);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (heartSource)
        {
            heartSource.volume = targetVolume;
            audioMixer.SetFloat("drymixEcho", targetDrymixEchoOnValue);
        }
    }
    
    private IEnumerator FadeOutHeart(AudioSource heartSource, float duration, System.Action onComplete)
    {
        float startVolume = heartSource.volume;
        float timeElapsed = 0f;
        audioMixer.GetFloat("drymixEcho", out float startEcho);
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            if (heartSource)
            {
                heartSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.SmoothStep(0f, 1f, t));
                float newEcho = Mathf.Lerp(startEcho, 0f, Mathf.SmoothStep(0f, 1f, t));
                audioMixer.SetFloat("drymixEcho", newEcho);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (heartSource)
        {
            heartSource.volume = 0f;
            audioMixer.SetFloat("drymixEcho", 0f);
            heartSource.Stop();
        }
        onComplete?.Invoke();
    }
}
