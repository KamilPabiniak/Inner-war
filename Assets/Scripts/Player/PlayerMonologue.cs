using System.Collections;
using UnityEngine;

public class PlayerMonologue : PlayerModule
{
    [Header("Monologue Settings")]
    [SerializeField] private AudioSource monologueSource;

    private bool _isPlaying;
    private bool _wasInterrupted;

    private Coroutine _monologueCoroutine;

    private void OnEnable() => GameEvents.onPlayerDied += StopMonologue;

    private void OnDisable() => GameEvents.onPlayerDied -= StopMonologue;

    public void PlayMonologue(AudioClip clip, float volume = 1f, float delay = 0f)
    {
        if (clip == null)
        {
            return;
        }
        if (_isPlaying)
        {
            return;
        }
        _monologueCoroutine = StartCoroutine(PlayMonologueCoroutine(clip, volume, delay));
    }

    private IEnumerator PlayMonologueCoroutine(AudioClip clip, float volume, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        _isPlaying = true;
        monologueSource.clip = clip;
        monologueSource.volume = volume;
        monologueSource.Play();

        yield return new WaitForSeconds(clip.length);

        _isPlaying = false;
        _wasInterrupted = false;
    }

    private void StopMonologue()
    {
        if (_isPlaying)
        {
            if (_monologueCoroutine != null)
            {
                StopCoroutine(_monologueCoroutine);
                _monologueCoroutine = null;
            }

            monologueSource.Stop();
            _isPlaying = false;
            _wasInterrupted = true;
        }
    }

    public bool WasInterruptedAndClear()
    {
        if (!_wasInterrupted) return false;
        _wasInterrupted = false;
        return true;
    }
}
