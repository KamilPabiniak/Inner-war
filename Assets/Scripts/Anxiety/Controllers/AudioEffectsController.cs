using System.Collections;
using Anxiety.Effects;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectsController : MonoBehaviour
{
    [Header("Reference")]
    public AudioMixer audioMixer;

    [Header("Cut off Freq")]
    [SerializeField] private float cutOffOn = 350f;
    private Coroutine audioTransitionCoroutine = null;
    

    private void HandleFearEffect(BaseFearEffect effect, float duration)
    {
        ApplyAudioMuffle(duration);
    }

    private void ApplyAudioMuffle(float applyDuration)
    {
        if (audioTransitionCoroutine != null)
            StopCoroutine(audioTransitionCoroutine);
        audioTransitionCoroutine = StartCoroutine(TransitionCutoff(cutOffOn, applyDuration));
    }

    public void ResetAudioEffects(float applyDuration)
    {
        audioTransitionCoroutine = StartCoroutine(TransitionCutoff(22000f, applyDuration));
    }
    
    private IEnumerator TransitionCutoff(float targetValue, float duration)
    {
        audioMixer.GetFloat("cutoffFreq", out float startValue);
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float newValue = Mathf.Lerp(startValue, targetValue, timeElapsed / duration);
            audioMixer.SetFloat("cutoffFreq", newValue);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioMixer.SetFloat("cutoffFreq", targetValue);
    }
}