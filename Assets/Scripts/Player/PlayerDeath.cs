using System.Collections;
using UnityEngine;

public class PlayerDeath : PlayerModule
{
    public Transform checkpoint;
    
    [Header("Timing")]
    [Tooltip("Global time scale for death sequence")]
    [SerializeField] private float timeScale = 1f;

    [Header("Phase Durations (base values in seconds)")]
    [SerializeField] private float blackScreenBaseDuration = 2f;
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private float fadeOutFromDeathScreen = 1f;

    [Header("Audio Clips")]
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
        float blackDuration = blackScreenBaseDuration * timeScale;
        GameEvents.onBlackScreen.Invoke(0f, blackDuration, 0f);
        SoundFXManager.Instance.Play2DSFXClipDestroyOnIgnoreDeath(
            deadEnd, transform, 1f, deadEnd.length, false);
        yield return new WaitForSeconds(blackDuration);
        GameEvents.onDeathScreen?.Invoke();
        float respawnTime = this.respawnTime * timeScale;
        SoundFXManager.Instance.Play2DSFXClipDestroyOnIgnoreDeath(
            deadEoldProjectorSound, transform, 1f, respawnTime, true);
        Player.ToggleInput();
        Respawn();
        yield return new WaitForSeconds(respawnTime);

        // Fade-out black screen over scaled duration
        float fadeOutDuration = fadeOutFromDeathScreen * timeScale;
        GameEvents.onBlackScreen.Invoke(0f, 1f * timeScale, fadeOutDuration);
        Player.ToggleInput();

        // Sequence end
        _isDead = false;
        GameEvents.onPlayerRespawned?.Invoke();
    }
    
    private void Respawn()
    {
        Vector3 targetPosition = checkpoint != null ? checkpoint.position : _backupPos;
        transform.position = targetPosition;
    }
}
