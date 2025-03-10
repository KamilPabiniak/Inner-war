using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : PlayerModule
{
    public Transform checkpoint;
    [SerializeField] private float respawnTime = 3f;
    private Vector3 _backupPos;
    private bool _isDead;
    private void Start()
    {
        _backupPos = transform.position;
    }

    private void LateUpdate()
    {
        EnsureCorrectPositionAfterDeath();
    }
    
    private void EnsureCorrectPositionAfterDeath()
    {
        if (!_isDead) return;

        Vector3 targetPosition = checkpoint != null ? checkpoint.position : _backupPos;

        if (transform.position != targetPosition)
        {
            transform.position = targetPosition;
        }
    }
    

    [ContextMenu("TryKill")]
    public void Kill()
    {
        if (_isDead) return;
        _isDead = true; 
        GameEvents.onPlayerDied?.Invoke();
        StartCoroutine(HandleDeathState());
    }

    private IEnumerator HandleDeathState()
    {
        Player.ToggleInput();
        Respawn();
        yield return new WaitForSeconds(respawnTime);

        Player.ToggleInput();
        _isDead = false;
        GameEvents.onPlayerRespawned?.Invoke();
    }
    
    private void Respawn()
    {
        Vector3 targetPosition = checkpoint != null ? checkpoint.position : _backupPos;
        transform.position = targetPosition;
    }
}
