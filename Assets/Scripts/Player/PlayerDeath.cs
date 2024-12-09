using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : PlayerModule
{
    public Transform checkpoint;
    
    private void Respawn()
    {
        transform.position = checkpoint.position;
    }

    [ContextMenu("TryKill")]
    private void Kill()
    {
        StartCoroutine(DeathState());
    }

    private IEnumerator DeathState()
    {
        SceneManager.LoadSceneAsync(sceneBuildIndex:1 , LoadSceneMode.Additive);
        Respawn();
        yield return new WaitForSeconds(3f);
        SceneManager.UnloadSceneAsync(sceneBuildIndex:1);
    }
}
