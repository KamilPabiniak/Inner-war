using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;      
    public Camera camera1;        
    public Camera camera2;         
    public Button playButton;
    public Button exitButton;
    

    private void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;
        
        playButton.onClick.AddListener(OnPlayClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        SwitchCamera();
        GameEvents.onBlackScreen.Invoke(0f, 1f, 1f);
        menuUI.SetActive(false);
        GameEvents.onMenuExit?.Invoke();
    }

    private void OnExitClicked()
    {
        Debug.Log("Zamykanie gry...");
        Application.Quit();
    }

    private void SwitchCamera()
    {
        camera1.enabled = false;
        camera2.enabled = true;
        Destroy(camera1.gameObject);
    }
}