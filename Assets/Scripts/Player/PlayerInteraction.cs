using System;
using UnityEngine;

public interface IInteractable
{
    void Interact(Player player);
}

public class PlayerInteraction : PlayerModule
{
    [Header("Interaction Settings")]
    [Tooltip("Maksymalna odleg³oœæ, w jakiej mo¿na wejœæ w interakcjê")]
    public float interactionDistance = 3f;

    private PlayerInput _input;
    private string _lastObjectTag = "";
    private Collider _playerCollider;

    private void Start()
    {
        _input = GetComponent<PlayerInput>();
        _playerCollider = GetComponent<Collider>(); 
        if (_playerCollider == null)
            Debug.LogWarning("PlayerInteraction: nie znaleziono komponentu Collider na obiekcie Playera.");
    }

    private void LateUpdate()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
{
    Ray ray = new Ray(Player.cameraTransform.position, Player.cameraTransform.forward);
    Debug.DrawRay(Player.cameraTransform.position, Player.cameraTransform.forward * interactionDistance, Color.blue);
    
    RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance);
    if (hits.Length == 0)
    {
        if (!string.IsNullOrEmpty(_lastObjectTag))
        {
            _lastObjectTag = "";
            InteractionEventMessenger.HideInteractionText();
        }
        return;
    }

    Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
    
    RaycastHit? validHit = null;
    foreach (var hit in hits)
    {
        if (_playerCollider != null && hit.collider == _playerCollider)
            continue;
        validHit = hit;
        break;
    }

    if (validHit == null)
    {
        if (!string.IsNullOrEmpty(_lastObjectTag))
        {
            _lastObjectTag = "";
            InteractionEventMessenger.HideInteractionText();
        }
        return;
    }

    var hitInfo = validHit.Value;
    var interactable = hitInfo.collider.GetComponent<IInteractable>();
    if (interactable != null)
    {
        QuestSystem.QuestInteractable qi = hitInfo.collider.GetComponent<QuestSystem.QuestInteractable>();
        if (qi != null)
        {
            var currentQuest = QuestSystem.QuestManager.Instance.GetCurrentQuest();
            if (currentQuest == null || qi.associatedQuestID != currentQuest.questID)
            {
                if (!string.IsNullOrEmpty(_lastObjectTag))
                {
                    _lastObjectTag = "";
                    InteractionEventMessenger.HideInteractionText();
                }
                return;
            }
        }
        
        string objectTag = hitInfo.collider.tag;
        if (objectTag != _lastObjectTag)
        {
            _lastObjectTag = objectTag;
            InteractionEventMessenger.ShowInteractionText(objectTag);
        }

        if (_input.IsInteractPressed)
        {
            interactable.Interact(Player);
            _input.IsInteractPressed = false;
        }
    }
    else
    {
        if (!string.IsNullOrEmpty(_lastObjectTag))
        {
            _lastObjectTag = "";
            InteractionEventMessenger.HideInteractionText();
        }
    }
}

}
