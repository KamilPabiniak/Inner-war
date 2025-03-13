using System.Collections.Generic;
using System.Linq;
using Enemy;
using UnityEngine;

public class PlayerMakeNoise : PlayerModule
{
    [Header("Noise Settings")]
    public LayerMask targetMask;          // Layer mask for potential enemy targets
    public float noiseRange = 10f;        // The range within which noise is detected by enemies
    public float noiseCooldown = 3f;      // Time interval between noise emissions
    public float alertCooldownTime = 5f;  // Cooldown time before the same enemy can be alerted again

    [Header("Debugging")]
    public bool drawSphere = true;        // Draws the detection sphere in the editor for visualization

    private Collider[] _results;
    private Dictionary<Collider, float> _alertedTargets;
    private float _lastNoiseTime;

    private void Start()
    {
        _results = new Collider[10];
        _alertedTargets = new Dictionary<Collider, float>();
        _lastNoiseTime = -noiseCooldown;
    }

    private void Update()
    {
        ResetAlertedTargets();
        
        if (Player.state != Player.State.Walking) return;

        if (!(Time.time >= _lastNoiseTime + noiseCooldown)) return;
        if (Player.Instance.GetModule<PlayerMovement>().GetCrouch()) return;
        EmitNoise();
        _lastNoiseTime = Time.time;
    }

    private void EmitNoise()
    {
        int targetsInRange = Physics.OverlapSphereNonAlloc(transform.position, noiseRange, _results, targetMask);
        for (int i = 0; i < targetsInRange; i++)
        {
            var targetCollider = _results[i];
            var enemy = targetCollider.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;
            
            if (_alertedTargets.ContainsKey(targetCollider) && _alertedTargets[targetCollider] + alertCooldownTime > Time.time)
            {
                enemy.OnAttackCommandReceived(transform);
            }
            else
            {
                enemy.OnAlertReceived(transform.position);
                _alertedTargets[targetCollider] = Time.time;
            }
        }
    }

    private void ResetAlertedTargets()
    {
        List<Collider> targetsToRemove = new List<Collider>();
        foreach (var entry in _alertedTargets.Where(entry => entry.Value + alertCooldownTime <= Time.time))
        {
            targetsToRemove.Add(entry.Key);
        }
        foreach (var target in targetsToRemove)
        {
            _alertedTargets.Remove(target);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawSphere) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, noiseRange);
    }
}
