using System;
using UnityEngine;

namespace Checkpoint
{
    public class CheckpointArea : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Collider col;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            Player.Instance.GetModule<PlayerDeath>().checkpoint = spawnPoint;
            GameEvents.onSaveCheckpoint.Invoke();
            gameObject.SetActive(false);
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
