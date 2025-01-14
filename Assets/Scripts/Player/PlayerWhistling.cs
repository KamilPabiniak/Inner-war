using System.Collections.Generic;
using UnityEngine;

public class PlayerWhistling : PlayerModule
{
    public LayerMask targetMask;
    public float whistlingRange;
    public bool drawSphere;
    public float whistlingCooldown;
    public float alertCooldownTime = 5f;  
    [Header("References")] 
    public AudioSource source;
    public AudioClip whistlingSound;
    
    private bool _whistle;
    private bool _whistleTriggered;
    private PlayerInput _input;
    private Collider[] results;
    private Dictionary<Collider, float> alertedTargets;
    private float _lastWhistleTime;

    
    private void Start()
    {
        _input = GetComponent<PlayerInput>();
        results = new Collider[10];
        alertedTargets = new Dictionary<Collider, float>();
        _lastWhistleTime = -whistlingCooldown;
    }
    
    private void Update()
    {
        ResetAlertedTargets();

        if (Player.state != Player.State.Walking) return;

        HandleWhistling();

        if (_whistle && !_whistleTriggered)
        {
            if (Time.time >= _lastWhistleTime + whistlingCooldown)
            {
                Whistle();
                _whistleTriggered = true;  
                _lastWhistleTime = Time.time;
            }
        }
        else if (!_whistle) 
        {
            _whistleTriggered = false;
        }
    }

    private void HandleWhistling()
    {
        _whistle = _input.IsWhistlePressed;
    }
    
    private void Whistle()
    {
        source.PlayOneShot(whistlingSound);
        var targetsInRange = Physics.OverlapSphereNonAlloc(transform.position, whistlingRange, results, targetMask);
        
        for (int i = 0; i < targetsInRange; i++)
        {
            var targetCollider = results[i];
            var machineEnemy = targetCollider.GetComponent<MachineEnemy>();
            
            if (machineEnemy != null)
            {
                if (alertedTargets.ContainsKey(targetCollider))
                {
                    Debug.Log($"Target {targetCollider.name} already alerted. Last alerted at: {alertedTargets[targetCollider]}");
                }
                
                if (alertedTargets.ContainsKey(targetCollider) && alertedTargets[targetCollider] + alertCooldownTime > Time.time)
                {
                    machineEnemy.OnAttackCommandReceived(transform);  
                }
                else
                {
                    machineEnemy.OnAlertReceived(transform.position);
                    alertedTargets[targetCollider] = Time.time; 
                }
            }
        }
    }
    
    private void ResetAlertedTargets()
    {
        List<Collider> targetsToRemove = new List<Collider>();
        
        foreach (var entry in alertedTargets)
        {
            if (entry.Value + alertCooldownTime <= Time.time)
            {
                Debug.Log($"Resetting alert for {entry.Key.name}");
                targetsToRemove.Add(entry.Key); 
            }
        }
        
        foreach (var target in targetsToRemove)
        {
            alertedTargets.Remove(target);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!drawSphere) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, whistlingRange);
    }
}
