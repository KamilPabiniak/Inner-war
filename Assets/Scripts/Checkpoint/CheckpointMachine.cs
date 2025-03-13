using UnityEngine;

namespace Checkpoint
{
    public class CheckpointMachine : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform spawnPoint;
        public void Interact(Player player)
        {
            player.GetModule<PlayerDeath>().checkpoint = spawnPoint;
        }
    }
}
