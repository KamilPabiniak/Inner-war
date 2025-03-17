using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
 public class Player : MonoBehaviour
 { 
     public static Player Instance { get; private set; }
     [Header("Settings")] 
     [Tooltip("Wysokość gracza podczas stania.")]
     public float standingHeight = 2f;
     [Tooltip("Wysokość gracza podczas kucania.")]
     public float crouchHeight = 1f;
     [Tooltip("Przyspieszenie pod grawitacyjne.")]
     public float gravity = 20f;

     public State state;

    public enum State
    {
        Walking,
        Climbing
    }

    [Header("References")]
    public CharacterController characterController;
    public Transform cameraTransform;
    public CharacterController CharacterController => characterController;

    [Header("Modules")]
    [SerializeField] private PlayerModule[] modules;
    public PlayerInput Input { get; private set; }

    private bool InputEnabled { get; set; } = true;
    private bool GravityEnabled { get; set; } = true;
    private bool CharacterControllerEnabled { get; set; } = true;

    private float _verticalVelocity;
    private bool _isSettingsPanelActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        modules = GetComponents<PlayerModule>();
        foreach (var module in modules)
        {
            module.Initialize(this);
        }
    }

    private void Start()
    {
        Input = GetModule<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void OnEnable()
    {
        GameEvents.onTogglePanel += ToggleSettingsPanel;
    }

    private void OnDisable()
    {
        GameEvents.onTogglePanel -= ToggleSettingsPanel;
    }

    private void OnValidate()
    {
        if (characterController != null) { characterController.enabled = CharacterControllerEnabled; }
        if (Input != null) { Input.enabled = InputEnabled; }
    }

    private void Update()
    {
        if (characterController.enabled) { ApplyGround(); }
    }

    public T GetModule<T>() where T : PlayerModule
    {
        T module = modules.OfType<T>().FirstOrDefault();
        if (module == null)
            Debug.LogError($"Module of type {typeof(T).Name} not found!");
        return module;
    }

    private void ApplyGround()
    {
        if (!GravityEnabled) return;

        if (!characterController.isGrounded)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity, 0f);
        }

        Vector3 gravityMovement = Vector3.up * (_verticalVelocity * Time.deltaTime);
        characterController.Move(gravityMovement);
    }

    /// <summary>
    /// Przełącznik stanu CharacterController z Inspektora.
    /// </summary>
    [ContextMenu("Toggle CharacterController")]
    public void ToggleCharacterController()
    {
        CharacterControllerEnabled = !CharacterControllerEnabled;
        if (characterController != null)
        {
            characterController.enabled = CharacterControllerEnabled;
        
            if (!characterController.enabled)
            {
                _verticalVelocity = 0f;
            }
        }

        Debug.Log($"CharacterController is now {(CharacterControllerEnabled ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Przełącznik stanu wejścia gracza z Inspektora.
    /// </summary>
    [ContextMenu("Toggle Input")]
    public void ToggleInput()
    {
        InputEnabled = !InputEnabled;
        if (Input == null) return;
        Input.enabled = InputEnabled;
        Debug.Log($"Input is now {(InputEnabled ? "Enabled" : "Disabled")}");
    }

    public void SetInputEnabled(bool isEnabled)
    {
        InputEnabled = isEnabled;
        Input.enabled = InputEnabled;
        Debug.Log($"Input is now {(isEnabled ? "enabled" : "disabled")}");
    }
    

    /// <summary>
    /// Przełącznik stanu grawitacji z Inspektora.
    /// </summary>
    [ContextMenu("Toggle Gravity")]
    public void ToggleGravity()
    {
        GravityEnabled = !GravityEnabled;
        Debug.Log($"Gravity is now {(GravityEnabled ? "Enabled" : "Disabled")}");
    }

    public void SetGravityEnabled(bool isEnabled)
    {
        GravityEnabled  = isEnabled;
        
        Debug.Log($"Gravity is now {(isEnabled ? "Enabled" : "Disabled")}");
    }
     
    private void ToggleSettingsPanel()
    {
        _isSettingsPanelActive = !_isSettingsPanelActive;
        
        BlockPlayerInputExceptSettingsPanel(_isSettingsPanelActive);
        
        if (_isSettingsPanelActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        Debug.Log("Settings panel toggled. Input " + (_isSettingsPanelActive ? "blocked" : "unblocked") + " except SettingsPanel.");
    }
    
    public void BlockPlayerInputExceptSettingsPanel(bool block)
    {
        if (Input != null)
        {
            Input.BlockAllInputsExceptSettingsPanel(block);
        }
    }

}

