using System.Collections;
using UnityEngine;

public class Ambience : MonoBehaviour
{
    private AudioSource _ambience;
    [Header("Default Ambience")]
    [SerializeField] private AudioClip inGameAmbience;

    [Header("High Alert Ambiences (Investigate/Attack)")]
    [SerializeField] private AudioClip[] highAlertAmbiences;

    [Header("Fade Settings")]  
    [SerializeField] private float fadeInDuration = 1.0f;   
    [SerializeField] private float fadeOutDuration = 1.0f;

    private Coroutine _currentFadeRoutine;
    private float _defaultAudioVolume;
    private int _highAlertIndex;
    private bool _playerIsDead;

    private void OnEnable()
    {
        GameEvents.onPlayerDied += StopAmbience;
        GameEvents.onPlayerRespawned += PlayAmbience;
        GameEvents.onMenuExit += OnMenuExit;
        GameEvents.onHighAlertStart += PlayHighAlertAmbience;
        GameEvents.onHighAlertEnd += PlayDefaultAmbience;
    }

    private void OnDisable()
    {
        GameEvents.onPlayerDied -= StopAmbience;
        GameEvents.onPlayerRespawned -= PlayAmbience;
        GameEvents.onMenuExit -= OnMenuExit;
        GameEvents.onHighAlertStart -= PlayHighAlertAmbience;
        GameEvents.onHighAlertEnd -= PlayDefaultAmbience;
    }

    private void Start()
    {
        _ambience = GetComponent<AudioSource>();
        _defaultAudioVolume = _ambience.volume;
    }
    
    private void OnMenuExit()
    {
        StartCoroutine(SetNewAmbience(inGameAmbience));
    }
    
    private IEnumerator FadeInAudio(float duration)
    {
        if (_ambience.isPlaying) yield return null;
        _ambience.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _ambience.volume = Mathf.Lerp(_ambience.volume, _defaultAudioVolume, timer / duration);
            yield return null;
        }
        _ambience.volume = _defaultAudioVolume;
    }
    
    private IEnumerator FadeOutAudio(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _ambience.volume = Mathf.Lerp( _ambience.volume, 0f, timer / duration);
            yield return null;
        }
        _ambience.volume = 0;
        _ambience.Stop();
    }
    
    private void PlayAmbience()
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);
        _playerIsDead = false;
        _ambience.clip = inGameAmbience;

        _currentFadeRoutine = StartCoroutine(FadeInAudio(fadeInDuration));
    }
    
    private void StopAmbience()
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);
        _playerIsDead = true;

        _currentFadeRoutine = StartCoroutine(FadeOutAudio(fadeOutDuration));
    }

    private IEnumerator SetNewAmbience(AudioClip clip)
    {
        if (_currentFadeRoutine != null)
            StopCoroutine(_currentFadeRoutine);
        if (_playerIsDead) yield return null;

        yield return StartCoroutine(FadeOutAudio(fadeOutDuration));
        _ambience.clip = clip;
        yield return StartCoroutine(FadeInAudio(fadeInDuration));
    }
    
    private void PlayHighAlertAmbience()
    {
        if (highAlertAmbiences.Length == 0) return;
        var clip = highAlertAmbiences[_highAlertIndex];
        _highAlertIndex = (_highAlertIndex + 1) % highAlertAmbiences.Length;
        _currentFadeRoutine = StartCoroutine(SetNewAmbience(clip));
    }

    private void PlayDefaultAmbience()
    {
        if (_playerIsDead) return;
        _currentFadeRoutine = StartCoroutine(SetNewAmbience(inGameAmbience));
    }
}
