using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : PlayerModule
{
    public Transform checkpoint;
    private Vector3 backupPos;
    private bool isDead;
    private void Start()
    {
        backupPos = transform.position;
    }

    private void LateUpdate()
    {
        EnsureCorrectPositionAfterDeath();
    }
    
    private void EnsureCorrectPositionAfterDeath()
    {
        if (!isDead) return;

        Vector3 targetPosition = checkpoint != null ? checkpoint.position : backupPos;

        if (transform.position != targetPosition)
        {
            transform.position = targetPosition;
        }
    }
    

    [ContextMenu("TryKill")]
    public void Kill()
    {
        if (isDead) return;
        isDead = true; 
        GameEvents.onPlayerDied?.Invoke();
        StartCoroutine(HandleDeathState());
    }

    private IEnumerator HandleDeathState()
    {
        Player.ToggleInput();
        Respawn();
        yield return new WaitForSeconds(3f);

        Player.ToggleInput();
        isDead = false;
        GameEvents.onPlayerRespawned?.Invoke();
    }
    
    private void Respawn()
    {
        Vector3 targetPosition = checkpoint != null ? checkpoint.position : backupPos;
        transform.position = targetPosition;
    }
}
