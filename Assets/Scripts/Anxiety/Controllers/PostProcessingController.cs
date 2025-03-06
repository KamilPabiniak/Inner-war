using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Anxiety.Controllers
{
    public class PostProcessingController : MonoBehaviour
    {
        [Header("Profiles")]
        [SerializeField] private VolumeProfile level0;
        [SerializeField] private VolumeProfile level1;
        [SerializeField] private VolumeProfile level2;
        [SerializeField] private VolumeProfile level3;
        [SerializeField] private VolumeProfile level4;
        [SerializeField] private VolumeProfile level5;
    
        [Header("Volume")]
        [SerializeField] private Volume volume;
        
        private Coroutine _transitionCoroutine;

        private void UpdatePostProcessingProfile(int fearLevel)
        {
            switch (fearLevel)
            {
                case 0 when level0 == null:
                    return;
                case 0:
                    volume.profile = level0;
                    break;
                case 1 when level1 == null:
                    return;
                case 1:
                    volume.profile = level1;
                    break;
                case 2 when level2 == null:
                    return;
                case 2:
                    volume.profile = level2;
                    break;
                case 3 when level3 == null:
                    return;
                case 3:
                    volume.profile = level3;
                    break;
                case 4 when level4 == null:
                    return;
                case 4:
                    volume.profile = level4;
                    break;
                case 5 when level5 == null:
                    return;
                case 5:
                    volume.profile = level5;
                    break;
            }
        }


        public void TurnOnEffects(float duration)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            UpdatePostProcessingProfile(AnxietyManager.Instance.DetermineFearLevel());
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
