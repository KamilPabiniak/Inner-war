using UnityEngine;

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
}