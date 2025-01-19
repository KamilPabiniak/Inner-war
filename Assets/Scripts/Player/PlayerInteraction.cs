using UnityEngine;


public interface IInteractable
{
    void Interact(Player player);
}

public class PlayerInteraction : PlayerModule
{
    [Header("Interaction Settings")] public float interactionDistance = 3f;

    private PlayerInput _input;

    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void LateUpdate()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        Ray ray = new Ray(Player.cameraTransform.position, Player.cameraTransform.forward);
        Debug.DrawRay(Player.cameraTransform.position, Player.cameraTransform.forward * interactionDistance,
            Color.blue);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                string objectTag = hit.collider.tag;
                InteractionEventMessenger.ShowInteractionText(objectTag);

                if (_input.IsInteractPressed)
                {
                    interactable.Interact(Player);
                }
            }
            else
            {
                InteractionEventMessenger.HideInteractionText();
            }
        }
        else
        {
            InteractionEventMessenger.HideInteractionText();
        }
    }
}
