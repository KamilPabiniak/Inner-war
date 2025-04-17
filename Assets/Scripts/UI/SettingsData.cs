using UnityEngine;
using UnityEngine.UI;

public class SettingsData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Toggle toggleCrouchMode;
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("Defaults")]
    [Tooltip("Default mouse sensitivity if no PlayerPrefs value exists")]
    [SerializeField] private float defaultMouseSensitivity = 100f;

    private const string CrouchToggleKey   = "CrouchToggleEnabled";
    private const string MouseSensKey      = "MouseSensitivity";

    private PlayerInput _playerInput;
    private PlayerLook  _playerLook;

    private void Start()
    {
        _playerInput = Player.Instance.GetModule<PlayerInput>();
        _playerLook  = Player.Instance.GetModule<PlayerLook>();
        LoadSettings();
        
        toggleCrouchMode.onValueChanged.AddListener(OnToggleCrouchChanged);
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void LoadSettings()
    {
        // 1) Toggle crouch
        bool isToggleCrouch = PlayerPrefs.GetInt(CrouchToggleKey, 0) == 1;
        toggleCrouchMode.isOn = isToggleCrouch;
        _playerInput.ToggleCrouchMode = isToggleCrouch;

        // 2) Mouse sensitivity
        float sens = PlayerPrefs.GetFloat(MouseSensKey, defaultMouseSensitivity);
        mouseSensitivitySlider.value = sens;
        _playerLook.MouseSensitivity = sens;
    }

    private void OnToggleCrouchChanged(bool isOn)
    {
        PlayerPrefs.SetInt(CrouchToggleKey, isOn ? 1 : 0);
        PlayerPrefs.Save();

        _playerInput.ToggleCrouchMode = isOn;
        if (!isOn) _playerInput.ResetCrouchState();
    }

    private void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MouseSensKey, value);
        PlayerPrefs.Save();

        _playerLook.MouseSensitivity = value;
    }
}
