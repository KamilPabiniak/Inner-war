using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class BlackScreenManager : MonoBehaviour
    {
        [SerializeField] private RawImage blackScreen;

        private void OnEnable()
        {
            GameEvents.onBlackScreen += HandleBlackScreen;
        }

        private void OnDisable()
        {
            GameEvents.onBlackScreen -= HandleBlackScreen;
        }
    
        private void HandleBlackScreen(float fadeTime, float duration)
        {
            StartCoroutine(BlackScreenCoroutine(fadeTime, duration));
        }

        private IEnumerator BlackScreenCoroutine(float fadeTime, float duration)
        {
            yield return StartCoroutine(Fade(0f, 1f, fadeTime));
            yield return new WaitForSeconds(duration);
            yield return StartCoroutine(Fade(1f, 0f, fadeTime));
        }
    
        private IEnumerator Fade(float startAlpha, float endAlpha, float fadeTime)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeTime);
                if (blackScreen != null)
                {
                    Color color = blackScreen.color;
                    color.a = newAlpha;
                    blackScreen.color = color;
                }
                yield return null;
            }
            if (blackScreen != null)
            {
                Color color = blackScreen.color;
                color.a = endAlpha;
                blackScreen.color = color;
            }
        }
    }
}