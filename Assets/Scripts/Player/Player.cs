using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Wysokość gracza podczas stania.")]
        public float standingHeight = 2f;
        [Tooltip("Wysokość gracza podczas kucania.")]
        public float crouchHeight = 1f;
        [Tooltip("Redukcja prędkości podczas kucania (w %).")]
        [Range(0, 100)] public float crouchSpeedReduction = 50f;
        [Tooltip("Przyspieszenie pod grawitacyjne.")]
        public float gravity = 20f;

        public State state;

        public enum State
        {
            Walking,
            Climbing
        }

        [Header("References")]
        public Transform cameraTransform;
        public CharacterController characterController;
        public Transform CameraTransform => cameraTransform;
        public CharacterController CharacterController => characterController;
    
        [Header("Modules")]
        public PlayerModule[] modules;

        private PlayerInput _input;

        private bool InputEnabled { get; set; } = true;
        private bool GravityEnabled { get; set; } = true;
        private bool CharacterControllerEnabled { get; set; } = true;
    
        private float _verticalVelocity;

        private void Awake()
        {
            modules = GetComponents<PlayerModule>();
        }
    
        private void Start()
        {
            foreach (var module in modules)
            {
                module.Initialize(this);
            }
            _input = GetModule<PlayerInput>();
        }

        private void OnValidate()
        {
            if (characterController != null) { characterController.enabled = CharacterControllerEnabled; }
            if (_input != null) { _input.enabled = InputEnabled; }
        }

        private void Update()
        {
            if (characterController.enabled) { ApplyGround(); }
        }

        public T GetModule<T>() where T : PlayerModule
        {
            foreach (var module in modules)
            {
                if (module is T foundModule)
                {
                    return foundModule;
                }
            }
            return null;
        }
    
        private void ApplyGround()
        {
            if (!characterController.isGrounded)
            {
                _verticalVelocity -= gravity * Time.deltaTime;
            }
            else if (_verticalVelocity < 0)
            {
                _verticalVelocity = 0f;
            }

            if (!GravityEnabled) return;
            Gravity();
        }

        private void Gravity()
        {
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
            if (_input == null) return;
            _input.enabled = InputEnabled;
            Debug.Log($"Input is now {(InputEnabled ? "Enabled" : "Disabled")}");
        }
    
        public void SetInputEnabled(bool isEnabled)
        {
            InputEnabled = isEnabled;
            _input.enabled = InputEnabled;
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
    }
}
