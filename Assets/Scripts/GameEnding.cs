using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour, IInteractable
{
    private const string SceneName = "DemoEnd";
    private const float BlackScreenStartAlpha = 3f;
    private const float BlackScreenDuration = 5f;
    
    public void Interact(Player player)
    {
        Player.Instance.SetInputEnabled(false);
        StartCoroutine(LoadWithBlackScreen());
    }

    private IEnumerator LoadWithBlackScreen()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        loadOp.allowSceneActivation = false;
        
        GameEvents.onBlackScreen.Invoke(BlackScreenStartAlpha, BlackScreenDuration, 0f);
        yield return new WaitForSeconds(BlackScreenStartAlpha);

        while (loadOp.progress < 0.9f)
            yield return null;
        loadOp.allowSceneActivation = true;
    }
}
