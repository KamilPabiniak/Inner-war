using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;
    public Camera cameraMenu;
    public Camera cameraMain;
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
        cameraMenu.enabled = true;
        cameraMain.enabled = false;
        
        playButton.onClick.AddListener(OnPlayClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }
    
    private void OnPlayClicked()
    {
        StartGame();
    }
    
    private void SkipMenu() => StartCoroutine(StartGame());

    IEnumerator StartGame()
    {
        SwitchCamera();
        GameEvents.onBlackScreen.Invoke(2f, 1f, 1f);
        menuUI.SetActive(false);
        GameEvents.onMenuExit?.Invoke();
        yield return null;
    }

    private void OnExitClicked()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    private void SwitchCamera()
    {
        cameraMenu.enabled = false;
        cameraMain.enabled = true;
        Destroy(cameraMenu.gameObject);
    }
}