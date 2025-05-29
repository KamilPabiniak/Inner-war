using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioController : MonoBehaviour
{
    [Header("Target AudioSource")]
    [SerializeField] private AudioSource audioSource;

    [Header("Optional: Random Clips")]
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private bool randomizeClips = false;

    [Header("Timing")]
    [Tooltip("Minimalny czas miêdzy akcjami")]
    [SerializeField] private float minInterval = 5f;
    [Tooltip("Maksymalny czas miêdzy akcjami")]
    [SerializeField] private float maxInterval = 15f;

    [Header("Pitch (tempo) Range")]
    [Tooltip("Minimalny pitch")]
    [SerializeField] private float minPitch = 0.5f;
    [Tooltip("Maksymalny pitch")]
    [SerializeField] private float maxPitch = 2f;

    private float nextActionTime;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ScheduleNextAction();
    }

    void Update()
    {
        if (Time.time >= nextActionTime)
        {
            PerformRandomAction();
            ScheduleNextAction();
        }
    }

    private void ScheduleNextAction()
    {
        nextActionTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    private void PerformRandomAction()
    {
        // Wybieramy losowo jedn¹ z 6 akcji
        int action = Random.Range(0, 6);
        switch (action)
        {
            case 0:
                // Pause / Unpause
                if (audioSource.isPlaying)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
                Debug.Log("AudioSource: Pause/Unpause");
                break;

            case 1:
                // Stop (ustawia time = 0)
                audioSource.Stop();
                Debug.Log("AudioSource: Stop");
                break;

            case 2:
                // Play (od nowa)
                audioSource.Play();
                Debug.Log("AudioSource: Play from current/zero");
                break;

            case 3:
                // Przewijanie do losowej pozycji
                if (audioSource.clip != null)
                {
                    audioSource.time = Random.Range(0f, audioSource.clip.length);
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                    Debug.Log($"AudioSource: Seek to {audioSource.time:F2}s");
                }
                break;

            case 4:
                // Zmiana pitch (tempo + wysokoœæ)
                float newPitch = Random.Range(minPitch, maxPitch);
                audioSource.pitch = newPitch;
                Debug.Log($"AudioSource: Change pitch to {newPitch:F2}");
                break;

            case 5:
                // Za³aduj losowy klip i zagraj
                if (randomizeClips && clips != null && clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log($"AudioSource: Load & Play clip '{clip.name}'");
                }
                break;
        }
    }
}
