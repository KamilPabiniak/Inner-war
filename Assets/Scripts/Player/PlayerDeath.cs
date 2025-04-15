using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : PlayerModule
{
    public Transform checkpoint;
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private AudioClip deadEnd;
    [SerializeField] private AudioClip deadEoldProjectorSound;
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
       
        StartCoroutine(HandleDeathState());
    }

    private IEnumerator HandleDeathState()
    {
        GameEvents.onPlayerDied?.Invoke();
        GameEvents.onBlackScreen.Invoke(0f, 10f, 0f);
        SoundFXManager.Instance.Play2DSoundFXClip(deadEnd, gameObject.transform, 1f);
        yield return new WaitForSeconds(deadEnd.length - 2.5f);
        GameEvents.onDeathScreen?.Invoke();
        SoundFXManager.Instance.Play2DSoundFXClipDestroyOn(deadEoldProjectorSound, gameObject.transform, 1f, respawnTime, true);
        Player.ToggleInput();
        Respawn();
        yield return new WaitForSeconds(respawnTime);
        GameEvents.onBlackScreen.Invoke(0f, 1f, 1f);
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
