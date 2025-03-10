using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectsController : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Audio Source Prefabs")]
    [Binder("AudioSources")]
    [SerializeField] private GameObject voicesS; // Prefab for voices effect
    [SerializeField] private GameObject heartS;  // Prefab for heart effect

    // These variables hold the currently playing instances.
    private AudioSource currentVoices;
    private AudioSource currentHeart;
    
    #region Cutoff Frequency Transition Methods

    public void ApplyAudioSuppression(float cutOffOn, float applyDuration)
    {
        if (audioTransitionCoroutine != null)
            StopCoroutine(audioTransitionCoroutine);
        audioTransitionCoroutine = StartCoroutine(TransitionCutoff(cutOffOn, applyDuration));
    }
    
    public void ResetAudioSuppression(float applyDuration)
    {
        audioTransitionCoroutine = StartCoroutine(TransitionCutoff(22000f, applyDuration));
    }
    
    private Coroutine audioTransitionCoroutine = null;
    private IEnumerator TransitionCutoff(float targetValue, float duration)
    {
        audioMixer.GetFloat("cutoffFreq", out float startValue);
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float newValue = Mathf.Lerp(startValue, targetValue, Mathf.SmoothStep(0f, 1f, t));
            audioMixer.SetFloat("cutoffFreq", newValue);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioMixer.SetFloat("cutoffFreq", targetValue);
    }

    #endregion

    #region Voices Methods

    /// <summary>
    /// Instantiates a new voices effect instance, sets its parent, obtains its AudioSource,
    /// assigns the provided AudioClip, and plays it with a fade-in.
    /// </summary>
    /// <param name="voicesFadeInDuration">Fade-in duration (seconds).</param>
    /// <param name="voicesTargetVolume">Target volume (0–1).</param>
    /// <param name="voicesSound">AudioClip to play.</param>
    public void PlayVoices(float voicesFadeInDuration, float voicesTargetVolume, AudioClip voicesSound)
    {
        // If an instance is already playing, do nothing.
        if (currentVoices != null)
            return;
        
        // Instantiate the voices prefab and set its parent.
        GameObject instance = Instantiate(voicesS, transform);
        currentVoices = instance.GetComponent<AudioSource>();
        if (currentVoices == null)
        {
            Debug.LogError("The instantiated voices prefab does not have an AudioSource component.");
            return;
        }
        // Set the clip and start from volume 0.
        currentVoices.clip = voicesSound;
        currentVoices.volume = 0f;
        currentVoices.Play();
        StartCoroutine(FadeIn(currentVoices, voicesFadeInDuration, voicesTargetVolume));
    }
    
    /// <summary>
    /// Fades out the voices effect and, when finished, destroys the instance.
    /// </summary>
    /// <param name="voicesFadeOutDuration">Fade-out duration (seconds).</param>
    public void StopVoices(float voicesFadeOutDuration)
    {
        if (currentVoices != null)
        {
            StartCoroutine(FadeOutAndStop(currentVoices, voicesFadeOutDuration, () =>
            {
                currentVoices = null;
            }));
        }
    }


    #endregion

    #region Heart Methods

    /// <summary>
    /// Instantiates a new heart effect instance, sets its parent, obtains its AudioSource,
    /// sets the initial "drymixEcho" to off (0f), and plays it with a fade-in that also transitions the "drymixEcho" parameter.
    /// </summary>
    /// <param name="targetDrymixEchoOnValue">
    /// The target value for the "drymixEcho" parameter (e.g., 60f).
    /// </param>
    /// <param name="heartFadeInDuration">Fade-in duration (seconds).</param>
    /// <param name="heartTargetVolume">Target volume for the heart effect.</param>
    public void PlayHeart(float targetDrymixEchoOnValue, float heartFadeInDuration, float heartTargetVolume)
    {
        if (currentHeart != null)
            return;
        
        // Instantiate the heart prefab and set its parent.
        GameObject instance = Instantiate(heartS, transform);
        currentHeart = instance.GetComponent<AudioSource>();
        if (currentHeart == null)
        {
            return;
        }
        currentHeart.volume = 0f;
        // Set the initial drymixEcho to "off" (assumed 0f).
        audioMixer.SetFloat("drymixEcho", 0f);
        currentHeart.Play();
        StartCoroutine(FadeInHeart(currentHeart, heartFadeInDuration, heartTargetVolume, targetDrymixEchoOnValue));
    }
    
    /// <summary>
    /// Fades out the heart effect (including transitioning "drymixEcho" back to 0) and, when finished, destroys the instance.
    /// </summary>
    /// <param name="heartFadeOutDuration">Fade-out duration (seconds).</param>
    public void StopHeart(float heartFadeOutDuration)
    {
        if (currentHeart != null)
        {
            StartCoroutine(FadeOutHeart(currentHeart, heartFadeOutDuration, () =>
            {
                currentHeart = null;
            }));
        }
    }

    #endregion

    #region Audio Fading Coroutines

    private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        float startVolume = source.volume;
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
        Destroy(source.gameObject);
        onComplete?.Invoke();
    }
    
    private IEnumerator FadeInHeart(AudioSource heartSource, float duration, float targetVolume, float targetDrymixEchoOnValue)
    {
        float startVolume = heartSource.volume;
        float timeElapsed = 0f;
        audioMixer.GetFloat("drymixEcho", out float startEcho); // Should be 0 initially.
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            if (!heartSource) break;
            heartSource.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.SmoothStep(0f, 1f, t));
            float newEcho = Mathf.Lerp(startEcho, targetDrymixEchoOnValue, Mathf.SmoothStep(0f, 1f, t));
            audioMixer.SetFloat("drymixEcho", newEcho);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (!heartSource) yield return null;
        heartSource.volume = targetVolume;
        audioMixer.SetFloat("drymixEcho", targetDrymixEchoOnValue);
    }
    
    private IEnumerator FadeOutHeart(AudioSource heartSource, float duration, System.Action onComplete)
    {
        float startVolume = heartSource.volume;
        float timeElapsed = 0f;
        audioMixer.GetFloat("drymixEcho", out float startEcho);
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            if (heartSource == null) yield return null;
            heartSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.SmoothStep(0f, 1f, t));
            float newEcho = Mathf.Lerp(startEcho, 0f, Mathf.SmoothStep(0f, 1f, t));
            audioMixer.SetFloat("drymixEcho", newEcho);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        heartSource.volume = 0f;
        audioMixer.SetFloat("drymixEcho", 0f);
        heartSource.Stop();
        Destroy(heartSource.gameObject);
        onComplete?.Invoke();
    }

    #endregion
}
