using System.Collections;
using UnityEngine;

public class Ambience : MonoBehaviour
{
    private AudioSource _ambience;
    [SerializeField] private AudioClip inGameAmbience;
    [SerializeField] private float fadeInDuration = 1.0f;   
    [SerializeField] private float fadeOutDuration = 1.0f;

    private Coroutine _currentFadeRoutine;
    private float _defaultAudioVolume;

    private void OnEnable()
    {
        GameEvents.onPlayerDied += StopAmbience;
        GameEvents.onPlayerRespawned += PlayAmbience;
        GameEvents.onMenuExit += () => StartCoroutine(SetNewAmbience());
    }

    private void OnDisable()
    {
        GameEvents.onPlayerDied -= StopAmbience;
        GameEvents.onPlayerRespawned -= PlayAmbience;
        GameEvents.onMenuExit -= () => StartCoroutine(SetNewAmbience());
    }

    private void Start()
    {
        _ambience = GetComponent<AudioSource>();
    }
    
    private IEnumerator FadeInAudio(float duration)
    {
        _ambience.volume = 0f;
        _ambience.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _ambience.volume = Mathf.Lerp(0f, _defaultAudioVolume, timer / duration);
            yield return null;
        }
        _ambience.volume = _defaultAudioVolume;
    }
    
    private IEnumerator FadeOutAudio(float duration)
    {
        _defaultAudioVolume = _ambience.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _ambience.volume = Mathf.Lerp(_defaultAudioVolume, 0f, timer / duration);
            yield return null;
        }
        _ambience.volume = 0f;
        _ambience.Stop();
    }
    
    private void PlayAmbience()
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);

        _currentFadeRoutine = StartCoroutine(FadeInAudio(fadeInDuration));
    }
    
    private void StopAmbience()
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);

        _currentFadeRoutine = StartCoroutine(FadeOutAudio(fadeOutDuration));
    }

    private IEnumerator SetNewAmbience()
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);

        yield return StartCoroutine(FadeOutAudio(fadeOutDuration));
        
        _ambience.clip = inGameAmbience;

        yield return StartCoroutine(FadeInAudio(fadeInDuration));
    }
}
