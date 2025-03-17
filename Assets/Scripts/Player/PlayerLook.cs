using UnityEngine;

public enum CameraShakeType
{
    Perlin,
    Random,
    Directional
}

public class PlayerLook : PlayerModule
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float verticalClamp = 85f;

    [Header("Camera Bobbing")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 10f;
    public float bobbingStrength = 0.05f;
    
    [Header("Camera Settings")]
    [Tooltip("Base height of the camera relative to the player")]
    public float cameraHeight = 1.6f;
    
    
    private PlayerInput _input;
    private float _xRotation;
    private float _bobbingOffset;
    private float _bobbingTimer;
    
    private Quaternion _leanRotation = Quaternion.identity; 

    // Internal state for camera shake
    private bool _isShaking;
    private float _shakeTimer;
    private float _shakeTotalDuration;
    private Vector3 _shakeSeed;
    
    private float _shakeAmplitude;
    private float _shakeFrequency;
    private AnimationCurve _shakeFalloffCurve;
    private bool _shakePosition;
    private bool _shakeRotation;
    private CameraShakeType _shakeType;
    private Vector3 _shakeDirection;


    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }
    
    private void Update()
    {
        if (Player.state == Player.State.Walking)
        {
            HandleLook(_input.LookInput);

            if (enableBobbing)
                HandleCameraBobbing(_input.MoveInput);
        }
        else if (Player.state == Player.State.Climbing)
        {
            HandleClimbingLook();
        }
        
        if (_isShaking)
        {
            HandleCameraShake();
        }
    }

    private void HandleLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);
        
        // Set the basic camera rotation
        Player.cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0, 0) * _leanRotation;
        Player.CharacterController.transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCameraBobbing(Vector2 moveInput)
    {
        if (moveInput.magnitude > 0.1f)
        {
            _bobbingTimer += Time.deltaTime * bobbingSpeed;
            _bobbingOffset = Mathf.Sin(_bobbingTimer) * bobbingStrength;
        }
        else
        {
            _bobbingTimer = 0;
            _bobbingOffset = Mathf.Lerp(_bobbingOffset, 0, Time.deltaTime * bobbingSpeed);
        }

        Vector3 cameraPosition = Player.cameraTransform.localPosition;
        cameraPosition.y = cameraHeight + _bobbingOffset;
        Player.cameraTransform.localPosition = cameraPosition;
    }
    
    private void HandleClimbingLook()
    {
        Player.cameraTransform.localRotation = Quaternion.identity;
        _xRotation = 0;
    }

    /// <summary>
    /// Applies additional lean rotation.
    /// </summary>
    /// <param name="rotation">Rotation to apply.</param>
    public void ApplyLeanRotation(Quaternion rotation)
    {
        _leanRotation = rotation;
    }
    
    /// <summary>
    /// Starts the camera shake effect.
    /// </summary>
    public void StartCameraShake(
        float duration,
        float amplitude,
        float frequency,
        AnimationCurve falloffCurve,
        bool affectPosition,
        bool affectRotation,
        CameraShakeType shakeType,
        Vector3 shakeDirection)
    {
        _isShaking = true;
        _shakeTimer = 0f;
        _shakeTotalDuration = duration;
        _shakeAmplitude = amplitude;
        _shakeFrequency = frequency;
        _shakeFalloffCurve = falloffCurve;
        _shakePosition = affectPosition;
        _shakeRotation = affectRotation;
        this._shakeType = shakeType;
        this._shakeDirection = shakeDirection;
        _shakeSeed = new Vector3(Random.Range(0f, 100f),
                                Random.Range(0f, 100f),
                                Random.Range(0f, 100f));
    }
    
    /// <summary>
    /// Updates the camera shake effect by adding offsets to the camera's position and/or rotation.
    /// </summary>
    private void HandleCameraShake()
    {
        _shakeTimer += Time.deltaTime;
        float progress = _shakeTimer / _shakeTotalDuration;
        float damping = _shakeFalloffCurve.Evaluate(progress);
        float currentAmplitude = _shakeAmplitude * damping;
        
        // Calculate position offset
        Vector3 shakePosOffset = Vector3.zero;
        if (_shakePosition)
        {
            switch (_shakeType)
            {
                case CameraShakeType.Perlin:
                    {
                        float offsetX = (Mathf.PerlinNoise(_shakeSeed.x, Time.time * _shakeFrequency) * 2f - 1f);
                        float offsetY = (Mathf.PerlinNoise(_shakeSeed.y, Time.time * _shakeFrequency) * 2f - 1f);
                        float offsetZ = (Mathf.PerlinNoise(_shakeSeed.z, Time.time * _shakeFrequency) * 2f - 1f);
                        shakePosOffset = new Vector3(offsetX, offsetY, offsetZ) * currentAmplitude;
                    }
                    break;
                case CameraShakeType.Random:
                    {
                        shakePosOffset = new Vector3(
                            (Random.value * 2f - 1f),
                            (Random.value * 2f - 1f),
                            (Random.value * 2f - 1f)
                        ) * currentAmplitude;
                    }
                    break;
                case CameraShakeType.Directional:
                    {
                        // Use shakeDirection modulated by a sine wave for a periodic effect
                        shakePosOffset = _shakeDirection.normalized * (Mathf.Sin(Time.time * _shakeFrequency) * currentAmplitude);
                    }
                    break;
            }
        }
        
        // Calculate rotation offset
        Quaternion shakeRot = Quaternion.identity;
        if (_shakeRotation)
        {
            Vector3 shakeRotOffset = Vector3.zero;
            switch (_shakeType)
            {
                case CameraShakeType.Perlin:
                    {
                        float offsetX = (Mathf.PerlinNoise(_shakeSeed.x, Time.time * _shakeFrequency) * 2f - 1f);
                        float offsetY = (Mathf.PerlinNoise(_shakeSeed.y, Time.time * _shakeFrequency) * 2f - 1f);
                        float offsetZ = (Mathf.PerlinNoise(_shakeSeed.z, Time.time * _shakeFrequency) * 2f - 1f);
                        shakeRotOffset = new Vector3(offsetX, offsetY, offsetZ) * currentAmplitude;
                    }
                    break;
                case CameraShakeType.Random:
                    {
                        shakeRotOffset = new Vector3(
                            (Random.value * 2f - 1f),
                            (Random.value * 2f - 1f),
                            (Random.value * 2f - 1f)
                        ) * currentAmplitude;
                    }
                    break;
                case CameraShakeType.Directional:
                    {
                        // Use shakeDirection modulated by a sine wave for a periodic effect
                        shakeRotOffset = _shakeDirection.normalized * (Mathf.Sin(Time.time * _shakeFrequency) * currentAmplitude);
                    }
                    break;
            }
            shakeRot = Quaternion.Euler(shakeRotOffset);
        }
        
        // Apply shake offsets to the camera
        Player.cameraTransform.localPosition += shakePosOffset;
        Player.cameraTransform.localRotation *= shakeRot;
        
        // End the shake effect when the duration is reached
        if (_shakeTimer >= _shakeTotalDuration)
        {
            _isShaking = false;
        }
    }
}
