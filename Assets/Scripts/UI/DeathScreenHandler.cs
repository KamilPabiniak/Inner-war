using TMPro;
using UnityEngine;

public class DeathScreenHandler : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel; 
    [SerializeField] private TMP_Text deathText; 
    [SerializeField] private string[] deathMessages;

    private void OnEnable()
    {
        GameEvents.OnPlayerDied += ShowDeathPanel;
        GameEvents.OnPlayerRespawned += HideDeathPanel;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= ShowDeathPanel;
        GameEvents.OnPlayerRespawned -= HideDeathPanel;
    }

    private void ShowDeathPanel()
    {
        if (deathPanel == null) return;
        deathPanel.SetActive(true);
        if (deathMessages != null && deathMessages.Length > 0)
        {
            int randomIndex = Random.Range(0, deathMessages.Length); 
            deathText.text = deathMessages[randomIndex]; 
        }
        else
        {
            deathText.text = "Game Over"; 
        }
    }

    private void HideDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }
}
