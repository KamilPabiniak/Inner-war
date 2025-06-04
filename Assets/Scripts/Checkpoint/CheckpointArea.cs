using System;
using UnityEngine;

namespace Checkpoint
{
    public class CheckpointArea : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Collider col;
        [SerializeField] private bool doNotDestroy;
        private bool _playerIsDead = false;

        private void OnEnable()
        {
            GameEvents.onPlayerDied    += HandlePlayerDied;
            GameEvents.onPlayerRespawned += HandlePlayerRespawned;
        }

        private void OnDisable()
        {
            GameEvents.onPlayerDied    -= HandlePlayerDied;
            GameEvents.onPlayerRespawned -= HandlePlayerRespawned;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_playerIsDead) return;
            if (!other.gameObject.CompareTag("Player")) return;
            Player.Instance.GetModule<PlayerDeath>().checkpoint = spawnPoint;
            GameEvents.onSaveCheckpoint.Invoke();
            
            var playerThrow = Player.Instance.GetModule<PlayerThrow>();
            bool hadStone = false;
            if (playerThrow != null)
            {
                hadStone = playerThrow.HasStone;
            }
            Player.Instance.GetModule<PlayerDeath>().SetSavedHasStone(hadStone);
            
            if (doNotDestroy) return;
            gameObject.SetActive(false);
        }
        
        private void HandlePlayerDied()
        {
            _playerIsDead = true;
        }

        private void HandlePlayerRespawned()
        {
            _playerIsDead = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
          
            if (!col) return;
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }
        }
    }
}
