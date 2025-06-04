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
        // Tworzymy ray wychodz¹cy z kamery
        Ray ray = new Ray(Player.cameraTransform.position, Player.cameraTransform.forward);
        Debug.DrawRay(Player.cameraTransform.position, Player.cameraTransform.forward * interactionDistance, Color.blue);

        // Pobieramy wszystkie trafienia w zasiêgu
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance);
        if (hits.Length == 0)
        {
            // Je¿eli nie ma ¿adnych trafieñ, chowamy poprzedni tekst i resetujemy _lastObjectTag
            if (!string.IsNullOrEmpty(_lastObjectTag))
            {
                _lastObjectTag = "";
                InteractionEventMessenger.HideInteractionText();
            }
            return;
        }

        // Sortujemy trafienia po odleg³oœci rosn¹co
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Szukamy pierwszego trafienia, które nie jest naszym w³asnym colliderem
        RaycastHit? validHit = null;
        foreach (var hit in hits)
        {
            if (_playerCollider != null && hit.collider == _playerCollider)
                continue; // pomijamy w³asny collider
            validHit = hit;
            break;
        }

        if (validHit == null)
        {
            // Gdy wszystkie trafienia to tylko w³asny collider, traktujemy to jak brak trafienia
            if (!string.IsNullOrEmpty(_lastObjectTag))
            {
                _lastObjectTag = "";
                InteractionEventMessenger.HideInteractionText();
            }
            return;
        }

        // Je¿eli mamy poprawne trafienie (nie w³asny collider)
        var hitInfo = validHit.Value;
        var interactable = hitInfo.collider.GetComponent<IInteractable>();
        if (interactable != null)
        {
            string objectTag = hitInfo.collider.tag;
            if (objectTag != _lastObjectTag)
            {
                _lastObjectTag = objectTag;
                InteractionEventMessenger.ShowInteractionText(objectTag);
            }

            if (_input.IsInteractPressed)
            {
                interactable.Interact(Player);
                // Resetujemy flagê, ¿eby interakcja zosta³a wykonana tylko raz
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
