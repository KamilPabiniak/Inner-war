using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioController : MonoBehaviour
{
    [Header("Target AudioSource")]
    [SerializeField] private AudioSource audioSource;

    [Header("Optional: Random Clips")]
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private bool randomizeClips = false;

    [Header("Timing")]
    [SerializeField] private float minInterval = 5f;
    [SerializeField] private float maxInterval = 15f;

    [Header("Pitch (tempo) Range")]
    [SerializeField] private float minPitch = 0.5f;
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
        int action = Random.Range(0, 6);
        switch (action)
        {
            case 0:
                if (audioSource.isPlaying)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
                break;

            case 1:
                audioSource.Stop();
                break;

            case 2:
                audioSource.Play();
                break;

            case 3:
                if (audioSource.clip != null)
                {
                    audioSource.time = Random.Range(0f, audioSource.clip.length);
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                }
                break;

            case 4:
                float newPitch = Random.Range(minPitch, maxPitch);
                audioSource.pitch = newPitch;
                break;

            case 5:
                if (randomizeClips && clips != null && clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                break;
        }
    }
}
