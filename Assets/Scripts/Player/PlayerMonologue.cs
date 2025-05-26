using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMonologue : PlayerModule
{
    [Header("Monologue Settings")]
    [SerializeField] private AudioSource monologueSource;

    public bool isPlaying;
    private bool _wasInterrupted;

    private Coroutine _monologueCoroutine;
    public event Action OnMonologueStarted;
    private struct MonologueRequest
    {
        public AudioClip Clip;
        public float Volume;
        public float Delay;
    }
    
    private readonly Queue<MonologueRequest> _requestQueue = new();

    private void OnEnable() => GameEvents.onPlayerDied += StopMonologue;

    private void OnDisable() => GameEvents.onPlayerDied -= StopMonologue;
    

    public void PlayMonologue(AudioClip clip, float volume = 1f, float delay = 0f)
    {
        if (clip == null) { return; }
        _requestQueue.Enqueue(new MonologueRequest { Clip = clip, Volume = volume, Delay = delay });
        _monologueCoroutine ??= StartCoroutine(PlayMonologueCoroutine());
    }

    private IEnumerator PlayMonologueCoroutine()
    {
        isPlaying = true;
        while (_requestQueue.Count > 0)
        {
            var request = _requestQueue.Dequeue();
            
            if (request.Delay > 0f)
                yield return new WaitForSeconds(request.Delay);

            // Play clip
            monologueSource.clip = request.Clip;
            monologueSource.volume = request.Volume;
            monologueSource.outputAudioMixerGroup = SoundFXManager.Instance.MonologueMixer;
            monologueSource.Play();
            OnMonologueStarted?.Invoke();
            
            float timePlayed = 0f;
            while (timePlayed < request.Clip.length)
            {
                if (!monologueSource.isPlaying)
                    break;
                timePlayed += Time.deltaTime;
                yield return null;
            }
            _wasInterrupted = false;
        }

        isPlaying = false;
        _monologueCoroutine = null;
    }

    private void StopMonologue()
    {
        // Stop any current playback and clear queue
        if (_monologueCoroutine != null)
        {
            StopCoroutine(_monologueCoroutine);
            _monologueCoroutine = null;
        }

        if (monologueSource.isPlaying)
        {
            monologueSource.Stop();
        }

        _requestQueue.Clear();
        _wasInterrupted = true;
    }

    public bool WasInterruptedAndClear()
    {
        if (!_wasInterrupted) return false;
        _wasInterrupted = false;
        return true;
    }
}
