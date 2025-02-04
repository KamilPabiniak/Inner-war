using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Anxiety.Effects
{
    public class PostProcessingManager : MonoBehaviour
    {
        [Header("Profiles")]
        [SerializeField] private VolumeProfile slight;
        [SerializeField] private VolumeProfile moderate;
        [SerializeField] private VolumeProfile high;
        [SerializeField] private VolumeProfile extreme;
        [SerializeField] private VolumeProfile faint;
    
        [Header("Volume")]
        [SerializeField] private Volume volume;
        
        private Coroutine transitionCoroutine = null;

        public void TurnOnEffects(float applyDuration)
        {
            UpdatePostProcessingProfile(AnxietyManager.Instance.FearLevel);
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            
            transitionCoroutine = StartCoroutine(TransitionVolumeWeight(1f, applyDuration));
        }

        public void DisableEffects(float applyDuration)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            
            transitionCoroutine = StartCoroutine(TransitionVolumeWeight(0f, applyDuration));
        }
        
        private IEnumerator TransitionVolumeWeight(float targetWeight, float duration)
        {
            float startWeight = volume.weight;
            float timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                volume.weight = Mathf.Lerp(startWeight, targetWeight, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            volume.weight = targetWeight;
        }

        private void UpdatePostProcessingProfile(float fearLevel)
        {
            VolumeProfile selectedProfile;

            switch (fearLevel)
            {
                case <= 20:
                    return;
                case <= 40:
                    selectedProfile = slight != null ? slight : null;
                    break;
                case <= 60:
                    selectedProfile = moderate != null ? moderate : null;
                    break;
                case <= 80:
                    selectedProfile = high != null ? high : null;
                    break;
                case <= 99:
                    selectedProfile = extreme != null ? extreme : null;
                    break;
                default:
                    selectedProfile = faint != null ? faint : null;
                    break;
            }

            if (selectedProfile != null && volume.profile != selectedProfile)
            {
                volume.profile = selectedProfile;
            }
        }
    }
}
