using Enemy;
using UnityEngine;
using UnityEngine.AI;

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
            EnemyBrain enemyBrain = agent.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.OnAttackCommandReceived(Player.Instance.transform);
            }
        }
        
        if (!_alertTriggered)
        {
            _alertTriggered = true; 
            LayerMask enemyMask = LayerMask.GetMask("Enemy");
            Collider[] colliders = Physics.OverlapSphere(impactPosition, detectionRadius, enemyMask, QueryTriggerInteraction.Ignore);

            foreach (var col in colliders)
            {
                var brainParent = col.GetComponentInParent<EnemyBrain>();

                if (brainParent == null) continue;
                brainParent.OnAlertReceived(impactPosition);
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void Interact(Player player)
    {
        PlayerThrow throwModule = player.GetComponent<PlayerThrow>();
        if (throwModule != null && !throwModule.HasStone)
        {
            throwModule.PickupStone(gameObject);
        }
    }
}
