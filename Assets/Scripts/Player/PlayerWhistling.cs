using System.Collections.Generic;
using System.Linq;
using Enemy;
using UnityEngine;

public class PlayerWhistling : PlayerModule
{
    public LayerMask targetMask;
    public float whistlingRange;
    public float whistlingCooldown;
    public float alertCooldownTime = 5f;  
    public float volume = 1f;  
    [Header("References")] 
    public GameObject source;
    public AudioClip whistlingSoundClip;
    
    [Header("Debugging")]
    public bool drawSphere = true;
    
    private bool _whistle;
    private bool _whistleTriggered;
    private PlayerInput _input;
    private Collider[] _results;
    private Dictionary<Collider, float> _alertedTargets;
    private float _lastWhistleTime;

    
    private void Start()
    {
        _input = GetComponent<PlayerInput>();
        _results = new Collider[10];
        _alertedTargets = new Dictionary<Collider, float>();
        _lastWhistleTime = -whistlingCooldown;
    }
    
    private void Update()
    {
        ResetAlertedTargets();

        if (Player.state != Player.State.Walking) return;

        HandleWhistling();

        switch (_whistle)
        {
            case true when !_whistleTriggered:
            {
                if (Time.time >= _lastWhistleTime + whistlingCooldown)
                {
                    Whistle();
                    _whistleTriggered = true;  
                    _lastWhistleTime = Time.time;
                }

                break;
            }
            case false:
                _whistleTriggered = false;
                break;
        }
    }

    private void HandleWhistling()
    {
        _whistle = _input.IsWhistlePressed;
    }
    
    private void Whistle()
    {
        SoundFXManager.Instance.Play3DSoundFXClip(whistlingSoundClip, source.transform, volume, audioMixerGroup: SoundFXManager.Instance.LowPassMixer);
        var targetsInRange = Physics.OverlapSphereNonAlloc(transform.position, whistlingRange, _results, targetMask);
        
        for (int i = 0; i < targetsInRange; i++)
        {
            var targetCollider = _results[i];
            var enemy = targetCollider.GetComponentInParent<Enemy.EnemyBrain>();

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
            Debug.Log($"Resetting alert for {entry.Key.name}");
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, whistlingRange);
    }
}
