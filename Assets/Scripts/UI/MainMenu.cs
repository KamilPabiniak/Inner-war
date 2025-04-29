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
    [SerializeField] private Animator letter;
    private static readonly int Read = Animator.StringToHash("Read");
    private CameraMenuController _cameraMenuController;

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
        _cameraMenuController = cameraMenu.GetComponent<CameraMenuController>();
    }
    
    private void OnPlayClicked()
    {
        //StartCoroutine(StartGame());
        _cameraMenuController.StopAndReturnToInitial();
        letter.SetBool(Read, true);
    }

    public void LetterCollected()
    {
        Debug.Log("Game Starting");
        StartCoroutine(StartGame());
    }
    
    private void SkipMenu() => StartCoroutine(StartGame());

    private IEnumerator StartGame()
    {
        GameEvents.onBlackScreen.Invoke(2f, 1f, 2f);
        yield return new WaitForSeconds(2f);
        SwitchCamera();
        menuUI.SetActive(false);
        GameEvents.onMenuExit?.Invoke();
        letter.SetBool(Read, false);
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