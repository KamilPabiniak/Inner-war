using Enemy;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Rock : MonoBehaviour, IInteractable
{
    public float detectionRadius = 5f; 
    public AudioClip[] rockSound;

    // Flag to ensure enemy alert is triggered only once.
    private bool _alertTriggered;

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 impactPosition = transform.position;
        int randomSound = Random.Range(0, rockSound.Length);
        SoundFXManager.Instance.Play3DSoundFXClip(rockSound[randomSound], transform, 1f, audioMixerGroup: SoundFXManager.Instance.LowPassMixer);
        
        // If the rock directly hits an enemy, force it to attack.
        NavMeshAgent agent = collision.gameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            Enemy.EnemyBrain enemyBrain = agent.GetComponent<Enemy.EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.ForceAttack();
            }
        }
        
        // Trigger enemy alert only once upon the first collision.
        if (!_alertTriggered)
        {
            _alertTriggered = true; // Set flag immediately to prevent further alerts.
            Collider[] colliders = Physics.OverlapSphere(impactPosition, detectionRadius);
            
            // Use a HashSet to avoid alerting the same EnemyBase more than once.
            HashSet<Enemy.EnemyBrain> alertedEnemies = new HashSet<Enemy.EnemyBrain>();
            
            foreach (Collider col in colliders)
            {
                Enemy.EnemyBrain enemyBrain = col.GetComponentInParent<Enemy.EnemyBrain>();
                if (enemyBrain != null && !alertedEnemies.Contains(enemyBrain))
                {
                    enemyBrain.OnAlertReceived(impactPosition);
                    alertedEnemies.Add(enemyBrain);
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
