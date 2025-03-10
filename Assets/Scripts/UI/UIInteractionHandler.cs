using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIInteractionHandler : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject interactionTextPanel; 
        public TextMeshProUGUI interactionText;

        [Header("Interaction Settings")]
        public List<TagToTextMapping> interactionMappings;
        private Dictionary<string, string> _tagToTextDictionary;

        private void Awake()
        {
            _tagToTextDictionary = new Dictionary<string, string>();
            foreach (var mapping in interactionMappings)
            {
                if (!_tagToTextDictionary.ContainsKey(mapping.tagName))
                {
                    _tagToTextDictionary.Add(mapping.tagName, mapping.description);
                }
            }
        }

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
            if (_tagToTextDictionary.TryGetValue(objectTag, out string description))
                return description;
            return string.Empty;
        }
    }

    [System.Serializable]
    public class TagToTextMapping
    {
        public string tagName;
        public string description;
    }
}