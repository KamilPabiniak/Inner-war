using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMakeNoise : PlayerModule
{
    [Header("Noise Settings")]  
    public LayerMask targetMask;          // Layer mask for potential enemy targets
    public float noiseRange = 10f;        // The range within which noise is detected by enemies
    [Range(0.1f, 1f)]
    public float heightScale = 0.5f; 
    public float noiseCooldown = 3f;      // Time interval between noise emissions
    public float alertCooldownTime = 5f;  // Cooldown time before the same enemy can be alerted again

    [Header("Debugging")]
    public bool drawSphere = true;        // Draws the detection sphere in the editor for visualization

    private Collider[] _results;
    private Dictionary<Collider, float> _alertedTargets;
    private float _lastNoiseTime;
    private Vector3 _lastPosition;
    private bool _lastCrouch;
    private Player.State _lastState;

    private const float MoveThreshold = 0.01f;

    private void Start()
    {
        _results = new Collider[10];
        _alertedTargets = new Dictionary<Collider, float>();
        _lastNoiseTime = -noiseCooldown;
        _lastPosition = transform.position;
        _lastCrouch = Player.Instance.GetModule<PlayerMovement>().GetCrouch();
        _lastState = Player.state;
    }

    private void Update()
    {
        // Reset per-enemy cooldowns
        ResetAlertedTargets();

        bool isWalking = Player.state == Player.State.Walking;
        bool isCrouching = Player.Instance.GetModule<PlayerMovement>().GetCrouch();

        // If player state or crouch just changed, reset lastPosition to avoid noise from stance changes
        if (_lastCrouch != isCrouching || _lastState != Player.state)
        {
            _lastPosition = transform.position;
            _lastCrouch = isCrouching;
            _lastState = Player.state;
            return;
        }

        // Only generate noise when walking and not crouching
        if (!isWalking || isCrouching)
        {
            _lastPosition = transform.position;
            return;
        }

        // Respect global noise cooldown
        if (Time.time < _lastNoiseTime + noiseCooldown)
            return;

        // Calculate horizontal movement to ignore vertical/stance changes
        Vector3 delta = transform.position - _lastPosition;
        delta.y = 0f;
        if (delta.sqrMagnitude < MoveThreshold * MoveThreshold)
            return;

        EmitNoise();
        _lastNoiseTime = Time.time;
        _lastPosition = transform.position;
    }

    private void EmitNoise()
    {
        int targets = Physics.OverlapSphereNonAlloc(transform.position, noiseRange, _results, targetMask);
        float invHeight = 1f / heightScale;
        float rangeSqr = noiseRange * noiseRange;

        for (int i = 0; i < targets; i++)
        {
            Collider col = _results[i];
            
            Vector3 dir = col.transform.position - transform.position;
            dir.y *= invHeight;
            
            if (dir.sqrMagnitude > rangeSqr)
                continue;

            var enemy = col.GetComponentInParent<Enemy.EnemyBrain>();
            Debug.LogError(enemy);
            if (enemy == null)
                continue;

            if (_alertedTargets.TryGetValue(col, out float lastAlert) && lastAlert + alertCooldownTime > Time.time)
            {
                enemy.OnAttackCommandReceived(transform);
            }
            else
            {
                enemy.OnAlertReceived(transform.position);
                _alertedTargets[col] = Time.time;
            }
        }
    }

    private void ResetAlertedTargets()
    {
        var expired = _alertedTargets.Where(kv => kv.Value + alertCooldownTime <= Time.time)
                                     .Select(kv => kv.Key)
                                     .ToList();
        foreach (var key in expired)
            _alertedTargets.Remove(key);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawSphere) return;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1f, heightScale, 1f));
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(Vector3.zero, noiseRange);
        Gizmos.matrix = old;
    }
}
