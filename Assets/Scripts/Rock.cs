using System;
using System.Collections;
using Enemy;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Rock : MonoBehaviour, IInteractable
{
    public float detectionRadius = 5f; 
    public AudioClip[] rockSound;

    // Flag to ensure enemy alert is triggered only once.
    private bool _alertTriggered;
    public bool canPlaySound = false;

    private MeshRenderer _meshRenderer;
    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        GameEvents.onPlayerDied += SetActive;
    }

    private void OnDisable()
    {
        GameEvents.onPlayerDied -= SetActive;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canPlaySound) return;
        Vector3 impactPosition = transform.position;
        int randomSound = Random.Range(0, rockSound.Length);
        SoundFXManager.Instance.Play3DSoundFXClip(rockSound[randomSound], transform, 1f, audioMixerGroup: SoundFXManager.Instance.LowPassMixer);
        
        NavMeshAgent agent = collision.gameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            EnemyBrain enemyBrain = agent.GetComponent<EnemyBrain>();
            if (enemyBrain != null)
            {
                enemyBrain.OnAttackCommandReceived(Player.Instance.transform);
            }
        }

        if (_alertTriggered) return;
        _alertTriggered = true; 
        LayerMask enemyMask = LayerMask.GetMask("Enemy");
        Collider[] colliders = Physics.OverlapSphere(impactPosition, detectionRadius, enemyMask, QueryTriggerInteraction.Ignore);

        foreach (var col in colliders)
        {
            var brainParent = col.GetComponentInParent<EnemyBrain>();

            if (brainParent == null) continue;
            brainParent.OnAlertReceived(impactPosition);
        }
        
        Destroy(gameObject, 1f);
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

    public void SetUnactive()
    {
        _meshRenderer.enabled = false;
        _boxCollider.enabled = false;
        _rigidbody.isKinematic = true;
    }

    private void SetActive()
    {
        _meshRenderer.enabled = true;
        _boxCollider.enabled = true;
        _rigidbody.isKinematic = false;
    }
}
