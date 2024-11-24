using UnityEngine;


public interface IInteractable
{
    void Interact(Player player);
}
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    private Player _player;
    private PlayerInput _input;

    private void Start()
    {
        _player = GetComponent<Player>();
        _input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (!_player.InputEnabled) return;
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (_input.IsInteractPressed)
        {
            Ray ray = new Ray(_player.cameraTransform.position, _player.cameraTransform.forward);
            Debug.DrawRay(_player.cameraTransform.position, _player.cameraTransform.forward * interactionDistance, Color.blue);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact(_player);
            }
        }
    }
}
