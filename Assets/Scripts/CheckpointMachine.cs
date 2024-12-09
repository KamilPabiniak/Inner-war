using UnityEngine;

public class CheckpointMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _spawnpoint;
    public void Interact(Player player)
    {
        player.GetModule<PlayerDeath>().checkpoint = _spawnpoint;
    }
}
