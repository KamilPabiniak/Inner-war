using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactionTextPanel; 
    public TextMeshProUGUI interactionText;

    [Header("Interaction Settings")]
    public List<TagToTextMapping> interactionMappings;

    private void OnEnable()
    {
        InteractionEventMessenger.OnShowInteractionText += HandleShowInteractionText;
        InteractionEventMessenger.OnHideInteractionText += HandleHideInteractionText;
    }

    private void OnDisable()
    {
        InteractionEventMessenger.OnShowInteractionText -= HandleShowInteractionText;
        InteractionEventMessenger.OnHideInteractionText -= HandleHideInteractionText;
    }

    private void HandleShowInteractionText(string objectTag)
    {
        string description = GetDescriptionForTag(objectTag);

        if (!string.IsNullOrEmpty(description))
        {
            interactionText.text = description;
            interactionTextPanel.SetActive(true);
        }
    }

    private void HandleHideInteractionText()
    {
        interactionTextPanel.SetActive(false);
    }

    private string GetDescriptionForTag(string objectTag)
    {
        foreach (var mapping in interactionMappings)
        {
            if (mapping.tagName == objectTag)
            {
                return mapping.description;
            }
        }

        return string.Empty; 
    }
}

[System.Serializable]
public class TagToTextMapping
{
    public string tagName;
    public string description;
}