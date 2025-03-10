using Enemy;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Rock : MonoBehaviour, IInteractable
{
    public float detectionRadius = 5f; 
    public AudioClip rockSound;

    // Flag to ensure enemy alert is triggered only once.
    private bool _alertTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 impactPosition = transform.position;
        SoundFXManager.Instance.PlaySoundFXClip(rockSound, transform, 1f);
        
        // If the rock directly hits an enemy, force it to attack.
        NavMeshAgent agent = collision.gameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            EnemyBase enemy = agent.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.ForceAttack();
            }
        }
        
        // Trigger enemy alert only once upon the first collision.
        if (!_alertTriggered)
        {
            _alertTriggered = true; // Set flag immediately to prevent further alerts.
            Collider[] colliders = Physics.OverlapSphere(impactPosition, detectionRadius);
            
            // Use a HashSet to avoid alerting the same EnemyBase more than once.
            HashSet<EnemyBase> alertedEnemies = new HashSet<EnemyBase>();
            
            foreach (Collider col in colliders)
            {
                EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
                if (enemy != null && !alertedEnemies.Contains(enemy))
                {
                    Debug.LogError("Kurwa alarm!");
                    enemy.OnAlertReceived(impactPosition);
                    alertedEnemies.Add(enemy);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Implementation of IInteractable so the rock can be picked up.
    public void Interact(Player player)
    {
        PlayerThrow throwModule = player.GetComponent<PlayerThrow>();
        if (throwModule != null && !throwModule.HasStone)
        {
            throwModule.PickupStone(gameObject);
        }
    }
}
