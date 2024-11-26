using UnityEngine;


public interface IInteractable
{
    void Interact(Player player);
}
public class PlayerInteraction : PlayerModule
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    
    private PlayerInput _input;

    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (_input.IsInteractPressed)
        {
            Ray ray = new Ray(Player.cameraTransform.position, Player.cameraTransform.forward);
            Debug.DrawRay(Player.cameraTransform.position, Player.cameraTransform.forward * interactionDistance, Color.blue);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact(Player);
            }
        }
    }
}
