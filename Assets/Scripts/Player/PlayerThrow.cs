using UnityEngine;

public class PlayerThrow : PlayerModule
{
    [Header("Rock Throw Settings")]
    public Transform handPosition;
    public GameObject stonePrefab;
    public float throwForce = 10f;

    [Header("Trajectory Settings")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public Color trajectoryColor = Color.yellow;
    public float trajectoryWidth = 0.05f;
    
    [Header("Ammo Settings")]
    public bool infiniteAmmo = false;

    private PlayerInput _playerInput;
    private bool canThrow;
    private bool hasStone = true; // Ammo system: only one stone at a time.

    public bool HasStone => hasStone;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.OnStartAiming += StartAiming;
        _playerInput.OnStopAiming += StopAiming;
        _playerInput.OnThrowStone += HandleThrowInput;

        SetupTrajectoryLine();
    }

    private void OnDestroy()
    {
        _playerInput.OnStartAiming -= StartAiming;
        _playerInput.OnStopAiming -= StopAiming;
        _playerInput.OnThrowStone -= HandleThrowInput;
    }

    private void Update()
    {
        if (_playerInput.IsAiming)
        {
            VisualizeTrajectory();
        }
        else
        {
            trajectoryLine.positionCount = 0;
        }
    }

    private void SetupTrajectoryLine()
    {
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth;
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;
        trajectoryLine.positionCount = 0;
    }

    private void StartAiming()
    {
        if (hasStone)
        {
            canThrow = true;
            trajectoryLine.positionCount = trajectoryResolution;
        }
    }

    private void StopAiming()
    {
        canThrow = false;
        trajectoryLine.positionCount = 0;
    }

    private void HandleThrowInput()
    {
        if (canThrow && hasStone)
        {
            ThrowStone();
            if (!infiniteAmmo)
            {
                hasStone = false; 
            }
            trajectoryLine.positionCount = 0;
        }
    }

    private void ThrowStone()
    {
        GameObject stone = Instantiate(stonePrefab, handPosition.position, Quaternion.identity);
        Rigidbody rb = stone.GetComponent<Rigidbody>();

        Vector3 targetPoint = GetAimPoint();
        Vector3 throwDirection = (targetPoint - handPosition.position).normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    // Calculates the maximum throw distance based on the camera's aim direction using projectile physics.
    private float CalculateMaxThrowDistance(Vector3 aimDirection)
    {
        float v = throwForce; // initial speed
        float g = Physics.gravity.magnitude;
        // Determine the angle (theta) relative to the horizontal plane.
        float theta = Mathf.Atan2(aimDirection.y, new Vector2(aimDirection.x, aimDirection.z).magnitude);
        // Assume ground level at y = 0; h is the hand's height.
        float h = handPosition.position.y;
        // Time of flight (projectile motion from height h).
        float time = (v * Mathf.Sin(theta) + Mathf.Sqrt(v * v * Mathf.Sin(theta) * Mathf.Sin(theta) + 2 * g * h)) / g;
        float range = v * Mathf.Cos(theta) * time;
        return range;
    }

    // Determines the aim point based on the camera's forward direction and the maximum throw distance.
    private Vector3 GetAimPoint()
    {
        Vector3 aimDirection = Player.cameraTransform.forward;
        float maxDistance = CalculateMaxThrowDistance(aimDirection);
        return handPosition.position + aimDirection * maxDistance;
    }

    private void VisualizeTrajectory()
    {
        Vector3[] points = new Vector3[trajectoryResolution];
        Vector3 startPoint = handPosition.position;
        Vector3 velocity = GetThrowDirection() * throwForce;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i / (float)trajectoryResolution;
            points[i] = CalculatePointPosition(startPoint, velocity, time);
        }

        trajectoryLine.SetPositions(points);
    }

    private Vector3 CalculatePointPosition(Vector3 startPoint, Vector3 velocity, float time)
    {
        return startPoint + velocity * time + 0.5f * Physics.gravity * time * time;
    }

    private Vector3 GetThrowDirection()
    {
        Vector3 targetPoint = GetAimPoint();
        return (targetPoint - handPosition.position).normalized;
    }

    // Called when the player picks up a stone via interaction.
    public void PickupStone(GameObject stone)
    {
        if (!hasStone)
        {
            hasStone = true;
            // Optionally, you can add feedback (sound, animation, etc.)
            Destroy(stone);
        }
    }
}
