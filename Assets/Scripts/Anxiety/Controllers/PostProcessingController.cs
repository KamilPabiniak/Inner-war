using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Anxiety.Controllers
{
    public class PostProcessingController : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField] private Volume volume;
        
        private Coroutine _transitionCoroutine;

        public void TurnOnEffects(float duration, VolumeProfile profile)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            volume.profile = profile;
            _transitionCoroutine = StartCoroutine(TransitionVolumeWeight(1f, duration));
        }

        public void TurnOffEffects(float duration)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(TransitionVolumeWeight(0f, duration));
        }
    
        private IEnumerator TransitionVolumeWeight(float targetWeight, float duration)
        {
            float startWeight = volume.weight;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                volume.weight = Mathf.Lerp(startWeight, targetWeight, Mathf.SmoothStep(0f, 1f, t));
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            volume.weight = targetWeight;
        }

    }
}
