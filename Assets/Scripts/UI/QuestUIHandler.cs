using System.Collections;
using QuestSystem;
using TMPro;
using UnityEngine;

public class QuestUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questInfo;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayTime = 2f;
    private void OnEnable() => QuestManager.Instance.OnQuestUpdated += ShowInfoQuest;
    private void OnDisable() => QuestManager.Instance.OnQuestUpdated -= ShowInfoQuest;
    
    private void ShowInfoQuest(Quest data)
    {
        StartCoroutine(ShowText());
    }
    
    IEnumerator ShowText()
    {
        Color textColor = questInfo.color;
        float startAlpha = textColor.a;
        float elapsedTime = 0f;

        // Fade In
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            questInfo.color = textColor;
            yield return null;
        }
        
        textColor.a = 1f;
        questInfo.color = textColor;
        
        // Wait
        yield return new WaitForSeconds(displayTime);
        
        // Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(1f, startAlpha, elapsedTime / fadeDuration);
            questInfo.color = textColor;
            yield return null;
        }
        
        textColor.a = 0f;
        questInfo.color = textColor;
    }
}
