using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
 public class Player : MonoBehaviour
 { 
     public static Player Instance { get; private set; }
     
     [Header("Settings")]
     [Tooltip("Standing height of the player.")]
     [SerializeField] private float standingHeight = 2f;
     [Tooltip("Crouch height of the player.")]
     [SerializeField] private float crouchHeight = 1f;
     [Tooltip("Gravity acceleration.")]
     [SerializeField] private float gravity = 20f;
     [Tooltip("Fall distance threshold causing death.")]
     [SerializeField] private float fallDamageThreshold = 10f;

    public enum State { Walking, Climbing }
    public State state;

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

    private bool _isFalling;
    private bool _enableFallingDmg;
    private float _fallStartHeight;
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
        ApplyHeight(standingHeight);
    }
    
    private void OnEnable()
    {
        GameEvents.onTogglePanel += ToggleSettingsPanel;
        GameEvents.onMenuExit += StartPlay;
    }

    private void OnDisable()
    {
        GameEvents.onTogglePanel -= ToggleSettingsPanel;
        GameEvents.onMenuExit -= StartPlay;
    }

    private void OnValidate()
    {
        if (characterController != null) { characterController.enabled = CharacterControllerEnabled; }
        if (Input != null) { Input.enabled = InputEnabled; }
    }

    private void Update()
    {
        if (characterController.enabled) { ApplyGround(); CheckFallDamage();}
    }

    public T GetModule<T>() where T : PlayerModule
    {
        T module = modules.OfType<T>().FirstOrDefault();
        if (module == null)
            Debug.LogError($"Module of type {typeof(T).Name} not found!");
        return module;
    }

    private void ApplyHeight(float newHeight) => characterController.height = newHeight;

    public void ApplyCrouch(bool isCrouching)
        => ApplyHeight(isCrouching ? crouchHeight : standingHeight);

    private void StartPlay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GetModule<PlayerInput>().enabled = true;
        _enableFallingDmg = true;
    }
    
    private void CheckFallDamage()
    {
        if (!characterController.isGrounded && state == State.Walking && _enableFallingDmg)
        {
            if (!_isFalling)
            {
                _isFalling = true;
                _fallStartHeight = transform.position.y;
            }
        }
        else if (_isFalling)
        {
            float fallDistance = _fallStartHeight - transform.position.y;
            if (fallDistance >= fallDamageThreshold)
            {
                var deathModule = GetModule<PlayerDeath>();
                if (deathModule != null)
                {
                    deathModule.Kill();
                }
            }
            _isFalling = false;
        }
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

    private void BlockPlayerInputExceptSettingsPanel(bool block)
    {
        if (Input != null)
        {
            Input.BlockAllInputsExceptSettingsPanel(block);
        }
    }

}

