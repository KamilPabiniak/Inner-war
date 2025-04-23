using UnityEngine;

public enum CameraShakeType { Perlin, Random, Directional }

[RequireComponent(typeof(Player))]
public class PlayerLook : PlayerModule
{
    [Header("Camera Position & Collision Offset")]
    [Tooltip("Y = eye height offset; Z = collision push back distance")]
    [SerializeField] private Vector3 cameraOffset = new(0f, 0.1f, 0.05f);

    [Header("Follow Settings")]
    [Tooltip("How quickly the camera follows the target position (higher = snappier)")]
    [SerializeField] private float cameraFollowSpeed = 5f;

    [Header("Mouse Settings")]
    [Tooltip("Horizontal and vertical look sensitivity multiplier")]
    [SerializeField] private float mouseSensitivity = 100f;
    [Tooltip("Maximum vertical look angle in degrees")]
    [SerializeField] private float verticalClamp = 85f;

    [Header("Camera Bobbing")]
    [SerializeField] private bool enableBobbing = true;
    [Tooltip("Speed of bobbing animation relative to movement speed")]
    [SerializeField] private float bobbingSpeed = 10f;
    [Tooltip("Amplitude of bobbing in world units (meters)")]
    [SerializeField] private float bobbingStrength = 0.05f;

    [Header("Collision Settings")]
    [Tooltip("Layers considered for camera collision checks")]
    [SerializeField] private LayerMask collisionMask;
    [Tooltip("Radius of the sphere used for collision detection")]
    [SerializeField] private float sphereRadius = 0.1f;

    [Header("Lean Settings")]
    [Tooltip("Maximum angle for leaning in degrees")]
    [SerializeField] private float leanAngle = 15f;
    [Tooltip("Maximum lateral offset for leaning")]
    [SerializeField] private float leanOffset = 0.2f;
    [Tooltip("Speed of lean transition")]
    [SerializeField] private float leanSpeed = 5f;

    [Header("Climb Look Settings")]
    [Tooltip("Speed of camera pitch adjustment when climbing")]
    [SerializeField] private float climbLookSpeed = 5f;
    
    // References & state
    private PlayerInput _playerInput;
    private CharacterController _characterController;
    private Transform _cameraTransform;

    private float _xRotation;
    private float _bobbingTimer;
    private float _bobbingOffset;

    private Vector3 _targetLeanPosition;
    private Quaternion _targetLeanRotation;
    private Vector3 _currentLeanPosition;
    private Quaternion _currentLeanRotation;

    // Shake state
    private bool _isShaking;
    private float _shakeElapsed;
    private float _shakeDuration;
    private float _shakeAmplitude;
    private float _shakeFrequency;
    private AnimationCurve _shakeFalloffCurve;
    private bool _shakePositionEnabled;
    private bool _shakeRotationEnabled;
    private CameraShakeType _shakeType;
    private Vector3 _shakeDirection;
    private Vector3 _shakeSeed;

    private void Start()
    {
        _playerInput = Player.GetModule<PlayerInput>();
        _characterController = Player.CharacterController;

        // Detach camera for free movement
        _cameraTransform = Player.cameraTransform;
        _cameraTransform.SetParent(null, true);
    }

    private void Update()
    {
        if (Player.state == Player.State.Walking)
        {
            HandleLeanInput();
            HandleMouseLook();
            if (enableBobbing)
                UpdateHeadBobbing(_playerInput.MoveInput);
        }
        else if (Player.state == Player.State.Climbing)
        {
            HandleClimbLook();
        }

        if (_isShaking)
            ProcessCameraShake();
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null)
            return;

        // Calculate pivot (eye position)
        Vector3 pivot = _characterController.transform.position
                        + _characterController.center
                        + Vector3.up * cameraOffset.y;

        // Determine world-right based on player yaw
        Vector3 yawRight = Quaternion.Euler(0f, Player.transform.eulerAngles.y, 0f) * Vector3.right;

        // Dynamic lean clamp
        float desiredLean = _currentLeanPosition.x;
        float leanSign = Mathf.Sign(desiredLean);
        float absLean = Mathf.Abs(desiredLean);
        float maxLean = absLean;

        if (absLean > 0f)
        {
            if (Physics.Raycast(
                pivot,
                yawRight * leanSign,
                out RaycastHit leanHit,
                absLean + sphereRadius,
                collisionMask,
                QueryTriggerInteraction.Ignore))
            {
                maxLean = Mathf.Max(0f, leanHit.distance - cameraOffset.z);
            }
        }
        Vector3 leanOffsetVec = yawRight * (maxLean * leanSign);

        // Initial follow target without bobbing
        Vector3 followTarget = pivot + leanOffsetVec;

        // Camera collision: SphereCast
        Vector3 dir = followTarget - pivot;
        float dist = dir.magnitude;
        Vector3 correctedFollow = followTarget;
        if (dist > 0f && Physics.SphereCast(
            pivot,
            sphereRadius,
            dir.normalized,
            out RaycastHit hit,
            dist,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            correctedFollow = hit.point + hit.normal * cameraOffset.z;
        }

        // OverlapSphere fallback
        Collider[] overlaps = Physics.OverlapSphere(
            correctedFollow,
            sphereRadius,
            collisionMask,
            QueryTriggerInteraction.Ignore);
        if (overlaps.Length > 0)
        {
            Vector3 push = Vector3.zero;
            foreach (var col in overlaps)
            {
                Vector3 closest = col.ClosestPoint(correctedFollow);
                float penetration = sphereRadius - Vector3.Distance(closest, correctedFollow);
                if (penetration > 0f)
                    push += (correctedFollow - closest).normalized * penetration;
            }
            correctedFollow += push;
        }
        
        Vector3 smoothPos = Vector3.Lerp(
            _cameraTransform.position,
            correctedFollow,
            cameraFollowSpeed * Time.deltaTime);

        // Apply bobbing offset in world units
        _cameraTransform.position = smoothPos + Vector3.up * _bobbingOffset;
        
        Quaternion baseRotation = Quaternion.Euler(
            _xRotation,
            Player.transform.eulerAngles.y,
            0f);
        _cameraTransform.rotation = baseRotation * _currentLeanRotation;
    }
    
    private void HandleClimbLook()
    {
        // wartoœæ od -1 do +1: -1 = schodzimy, +1 = wspinamy siê
        float moveY = Mathf.Clamp(_playerInput.MoveInput.y, -1f, 1f);
    
        // chcemy, aby przy ruchu w górê _xRotation by³o ujemne (patrzymy w górê),
        // wiêc mno¿ymy przez -verticalClamp
        float targetPitch = -verticalClamp * moveY;
    
        // p³ynne przejœcie od bie¿¹cej wartoœci do docelowej
        _xRotation = Mathf.Lerp(_xRotation, targetPitch, climbLookSpeed * Time.deltaTime);
    }
    
    public float MouseSensitivity
    {
        get => mouseSensitivity;
        set => mouseSensitivity = value;
    }

    private void HandleMouseLook()
    {
        Vector2 lookInput = _playerInput.LookInput;
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        _xRotation = Mathf.Clamp(_xRotation - mouseY, -verticalClamp, verticalClamp);
        Player.transform.Rotate(Vector3.up * mouseX);
    }

    private void UpdateHeadBobbing(Vector2 moveInput)
    {
        float speedFactor = moveInput.magnitude;
        if (speedFactor > 0f)
        {
            _bobbingTimer += Time.unscaledDeltaTime * bobbingSpeed;
            if (_bobbingTimer > Mathf.PI * 2f)
                _bobbingTimer -= Mathf.PI * 2f;

            _bobbingOffset = Mathf.Sin(_bobbingTimer) * bobbingStrength * Time.unscaledDeltaTime;
        }
        else
        {
            _bobbingOffset = Mathf.Lerp(_bobbingOffset, 0f, Time.unscaledDeltaTime * bobbingSpeed);
        }
    }

    private void HandleLeanInput()
    {
        if (_playerInput.IsLeanLeftPressed)
        {
            _targetLeanRotation = Quaternion.Euler(0f, 0f, leanAngle);
            _targetLeanPosition.x = -leanOffset;
        }
        else if (_playerInput.IsLeanRightPressed)
        {
            _targetLeanRotation = Quaternion.Euler(0f, 0f, -leanAngle);
            _targetLeanPosition.x = leanOffset;
        }
        else
        {
            _targetLeanRotation = Quaternion.identity;
            _targetLeanPosition.x = 0f;
        }

        _currentLeanRotation = Quaternion.Lerp(
            _currentLeanRotation,
            _targetLeanRotation,
            leanSpeed * Time.deltaTime);
        _currentLeanPosition = Vector3.Lerp(
            _currentLeanPosition,
            _targetLeanPosition,
            leanSpeed * Time.deltaTime);
    }

    private void ResetLookState()
    {
        _xRotation = 0f;
        _bobbingOffset = 0f;

        _currentLeanRotation = Quaternion.identity;
        _currentLeanPosition = Vector3.zero;
    }

    public void StartCameraShake(
        float duration,
        float amplitude,
        float frequency,
        AnimationCurve falloffCurve,
        bool affectPosition,
        bool affectRotation,
        CameraShakeType type,
        Vector3 direction)
    {
        _isShaking = true;
        _shakeElapsed = 0f;
        _shakeDuration = duration;
        _shakeAmplitude = amplitude;
        _shakeFrequency = frequency;
        _shakeFalloffCurve = falloffCurve;
        _shakePositionEnabled = affectPosition;
        _shakeRotationEnabled = affectRotation;
        _shakeType = type;
        _shakeDirection = direction.normalized;
        _shakeSeed = new Vector3(
            Random.value * 100f,
            Random.value * 100f,
            Random.value * 100f);
    }

    private void ProcessCameraShake()
    {
        _shakeElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_shakeElapsed / _shakeDuration);
        float damper = _shakeFalloffCurve.Evaluate(t);
        float currentAmp = _shakeAmplitude * damper;

        Vector3 posOffset = Vector3.zero;
        Quaternion rotOffset = Quaternion.identity;

        if (_shakePositionEnabled)
        {
            switch (_shakeType)
            {
                case CameraShakeType.Perlin:
                    posOffset = new Vector3(
                        Mathf.PerlinNoise(_shakeSeed.x, Time.time * _shakeFrequency) * 2f - 1f,
                        Mathf.PerlinNoise(_shakeSeed.y, Time.time * _shakeFrequency) * 2f - 1f,
                        Mathf.PerlinNoise(_shakeSeed.z, Time.time * _shakeFrequency) * 2f - 1f
                    ) * currentAmp;
                    break;
                case CameraShakeType.Random:
                    posOffset = Random.insideUnitSphere * currentAmp;
                    break;
                case CameraShakeType.Directional:
                    posOffset = _shakeDirection * (Mathf.Sin(Time.time * _shakeFrequency) * currentAmp);
                    break;
            }
        }

        if (_shakeRotationEnabled)
        {
            Vector3 eulerOffset;
            switch (_shakeType)
            {
                case CameraShakeType.Perlin:
                    eulerOffset = new Vector3(
                        Mathf.PerlinNoise(_shakeSeed.x, Time.time * _shakeFrequency) * 2f - 1f,
                        Mathf.PerlinNoise(_shakeSeed.y, Time.time * _shakeFrequency) * 2f - 1f,
                        Mathf.PerlinNoise(_shakeSeed.z, Time.time * _shakeFrequency) * 2f - 1f
                    ) * currentAmp;
                    break;
                case CameraShakeType.Random:
                    eulerOffset = Random.insideUnitSphere * currentAmp;
                    break;
                default:
                    eulerOffset = _shakeDirection * (Mathf.Sin(Time.time * _shakeFrequency) * currentAmp);
                    break;
            }
            rotOffset = Quaternion.Euler(eulerOffset);
        }

        _cameraTransform.localPosition += posOffset;
        _cameraTransform.localRotation *= rotOffset;

        if (_shakeElapsed >= _shakeDuration)
            _isShaking = false;
    }
}
