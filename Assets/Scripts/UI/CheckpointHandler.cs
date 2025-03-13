using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class CheckpointHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI checkpointInfo;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float displayTime = 2f;
    
        private void OnEnable() => GameEvents.onSaveCheckpoint += ShowInfoSave;
        private void OnDisable() => GameEvents.onSaveCheckpoint -= ShowInfoSave;

        private void ShowInfoSave()
        {
            StartCoroutine(ShowText());
        }

        IEnumerator ShowText()
        {
            Color textColor = checkpointInfo.color;
            float startAlpha = textColor.a;
            float elapsedTime = 0f;

            // Fade In
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                textColor.a = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
                checkpointInfo.color = textColor;
                yield return null;
            }
        
            textColor.a = 1f;
            checkpointInfo.color = textColor;
        
            // Wait
            yield return new WaitForSeconds(displayTime);
        
            // Fade Out
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                textColor.a = Mathf.Lerp(1f, startAlpha, elapsedTime / fadeDuration);
                checkpointInfo.color = textColor;
                yield return null;
            }
        
            textColor.a = 0f;
            checkpointInfo.color = textColor;
        }
    }
}