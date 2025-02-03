using System;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public float detectionRadius = 5f; 
    public AudioClip rockSound;

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 impactPosition = transform.position;
        SoundFXManager.Instance.PlaySoundFXClip(rockSound, transform, 1f);
        
        NavMeshAgent agent = collision.gameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            EnemyBase enemy = agent.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.ForceAttack();
            }
            return;
        }
        
        Collider[] colliders = Physics.OverlapSphere(impactPosition, detectionRadius);
        foreach (Collider col in colliders)
        {
            Debug.Log(col);
            EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.OnAlertReceived(impactPosition);
            }
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}