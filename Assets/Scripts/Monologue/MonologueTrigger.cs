using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MonologueTrigger : MonoBehaviour
{
    [Header("Monologue Settings")]
    [SerializeField] private AudioClip monologueClip;
    [SerializeField] private float monologueVolume = 1f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float delayBeforePlay;

    private bool _hasTriggered;
    private PlayerMonologue _cachedMonologue;

    private void OnEnable() => GameEvents.onPlayerRespawned += HandleRespawn;

    private void OnDisable() => GameEvents.onPlayerRespawned -= HandleRespawn;

    private void HandleRespawn()
    {
        if (_cachedMonologue != null && _cachedMonologue.WasInterruptedAndClear())
        {
            _hasTriggered = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (_hasTriggered && triggerOnce)
                return;

            var monologueModule = player.GetModule<PlayerMonologue>();
            if (monologueModule != null)
            {
                _cachedMonologue = monologueModule;
                monologueModule.PlayMonologue(monologueClip, monologueVolume, delayBeforePlay);
                _hasTriggered = true;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0.1f, 0.1f, 0.6f, 1f); 
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}