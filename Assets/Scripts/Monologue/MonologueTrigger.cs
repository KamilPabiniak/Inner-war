using Anxiety;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MonologueTrigger : MonoBehaviour
{
    [Header("Monologue Settings")]
    [SerializeField] private AudioClip monologueClip;
    [SerializeField] private float monologueVolume = 1f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float delayBeforePlay;
    [Tooltip("If enabled, triggers an active anxiety level on quest completion")]
    [SerializeField] private bool triggerAnxiety = false;
    [SerializeField, Range(5, 7), Tooltip("Anxiety level (5-7) to activate")]
    private int anxietyLevel = 5;
    [SerializeField, Tooltip("Duration in seconds for the active anxiety level")]
    private float anxietyDuration = 10f;

    [Space, Header("Extra time for activation after start game")] 
    [SerializeField] private float extraTime;

    private bool _hasTriggered;
    private Collider _trigger;
    private PlayerMonologue _cachedMonologue;

    private void Start()
    {
        _trigger = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        GameEvents.onPlayerRespawned += HandleRespawn;
        GameEvents.onMenuExit += HandleGameStart;
    }

    private void OnDisable()
    {
        GameEvents.onPlayerRespawned -= HandleRespawn;
        GameEvents.onMenuExit -= HandleGameStart;
    }

    private void HandleGameStart()
    {
        Invoke(nameof(SetUpTrigger),extraTime);
    }

    private void SetUpTrigger()
    {
        _trigger.enabled = true;
    }

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
            
            if (triggerAnxiety)
            {
                AnxietyManager.Instance.TriggerActiveTimed(anxietyLevel, anxietyDuration);
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