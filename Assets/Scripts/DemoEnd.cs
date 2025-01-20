using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoEnd : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text textEnd; // Tekst, który ma zyskaæ alphe 1
    public GameObject clickablePanel; // Panel, który obs³uguje klikniêcie

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (textEnd != null)
        {
            Color color = textEnd.color;
            color.a = 0f; // Ustawienie alpha na 0
            textEnd.color = color;
        }
        
        StartCoroutine(FadeInText());
    }

    private IEnumerator FadeInText()
    {
        yield return new WaitForSeconds(1f); // Poczekaj sekundê

        if (textEnd != null)
        {
            float duration = 2f; // Czas trwania efektu fade-in
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);

                Color color = textEnd.color;
                color.a = alpha; // Aktualizacja alpha
                textEnd.color = color;

                yield return null;
            }

            // Po zakoñczeniu efektu fade-in aktywuj panel
            if (clickablePanel != null)
            {
                clickablePanel.SetActive(true);
                AddClickListener(clickablePanel);
            }
        }
    }

    private void AddClickListener(GameObject panel)
    {
        Button button = panel.GetComponent<Button>();
        if (button == null)
        {
            button = panel.AddComponent<Button>();
        }

        button.onClick.AddListener(() => QuitGame());
    }

    private void QuitGame()
    {
        Debug.Log("Gra zostanie zamkniêta...");
        Application.Quit(); // Wy³¹cza grê (dzia³a w buildzie, nie w edytorze Unity)
    }
}