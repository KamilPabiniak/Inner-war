using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Name of the UI scene to load at start.")]
    public string uiSceneName = "UI";

    private void Awake()
    {
        LoadUIScene();
    }

    private void LoadUIScene()
    {
        if (!string.IsNullOrEmpty(uiSceneName))
        {
            if (!SceneManager.GetSceneByName(uiSceneName).isLoaded)
            {
                SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
                Debug.Log($"UI scene '{uiSceneName}' loaded successfully.");
            }
            else
            {
                Debug.LogWarning($"UI scene '{uiSceneName}' is already loaded.");
            }
        }
        else
        {
            Debug.LogError("UI scene name is not set in the GameManager.");
        }
    }
}
