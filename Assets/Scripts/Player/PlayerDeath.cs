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

    private void Update()
    {
        CheckIfIsRespawnedProperly();
    }
    
    private void CheckIfIsRespawnedProperly()
    {
        if (!isDead) return;

        if (checkpoint == null)
        {
            if (transform.position != backupPos)
            {
                transform.position = backupPos;
            }
        }
        else
        {
            if (transform.position != checkpoint.position)
            {
                transform.position = checkpoint.position;
            }
        }
    }
    

    [ContextMenu("TryKill")]
    public void Kill()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DeathState());
    }

    private IEnumerator DeathState()
    {
        Player.ToggleInput();
        var loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex:1 , LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOperation.isDone);
        
        Respawn();
        yield return new WaitForSeconds(3f);
        
        var unloadOperation = SceneManager.UnloadSceneAsync(sceneBuildIndex: 1);
        yield return new WaitUntil(() => unloadOperation.isDone);
        Player.ToggleInput();
        isDead = false;
    }
    
    private void Respawn()
    {
        if (checkpoint == null)
        {
            transform.position = backupPos;
            return;
        }
        transform.position = checkpoint.position;
    }
}
