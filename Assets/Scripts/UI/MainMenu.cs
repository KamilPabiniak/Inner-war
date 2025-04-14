using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;
    public Camera camera1;
    public Camera camera2;
    public Button playButton;
    public Button exitButton;

    private void OnEnable()
    {
        GameEvents.onSkipMenu += SkipMenu;
    }

    private void OnDisable()
    {
        GameEvents.onSkipMenu -= SkipMenu;
    }

    private void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;
        
        playButton.onClick.AddListener(OnPlayClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }
    
    private void OnPlayClicked()
    {
        StartGame();
    }

    
    private void SkipMenu()
    {
        StartGame();
    }
    
    private void StartGame()
    {
        SwitchCamera();
        GameEvents.onBlackScreen.Invoke(0f, 1f, 1f);
        menuUI.SetActive(false);
        GameEvents.onMenuExit?.Invoke();
    }

    private void OnExitClicked()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    /// <summary>
    /// Switches from camera1 to camera2.
    /// </summary>
    private void SwitchCamera()
    {
        camera1.enabled = false;
        camera2.enabled = true;
        Destroy(camera1.gameObject);
    }
}