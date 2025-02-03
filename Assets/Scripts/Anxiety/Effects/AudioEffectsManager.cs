using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Anxiety.Effects
{
    public class AudioEffectsManager : MonoBehaviour
    {
        public AudioMixer audioMixer;

        [Header("Cut off Freq")] 
        [SerializeField] private float cutOffOn = 350f;
        private Coroutine audioTransitionCoroutine = null;

        public void ApplyAudioMuffle(float applyDuration)
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
}
