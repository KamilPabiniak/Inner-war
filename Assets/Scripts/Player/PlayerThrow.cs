using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    [Header("Rzucanie Kamieniem")]
    public Transform handPosition;
    public GameObject stonePrefab;
    public LayerMask groundMask;
    public float throwForce = 10f;
    private bool canThrow = false;

    [Header("Trajektoria")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public Color trajectoryColor = Color.yellow;
    public float trajectoryWidth = 0.05f;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.OnStartAiming += StartAiming;
        playerInput.OnStopAiming += StopAiming;
        playerInput.OnThrowStone += HandleThrowInput;

        SetupTrajectoryLine();
    }

    private void OnDestroy()
    {
        playerInput.OnStartAiming -= StartAiming;
        playerInput.OnStopAiming -= StopAiming;
        playerInput.OnThrowStone -= HandleThrowInput;
    }

    private void Update()
    {
        if (playerInput.IsAiming)
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
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;
        trajectoryLine.positionCount = 0;
    }

    private void StartAiming()
    {
        canThrow = true; // Pozwala na wykonanie rzutu
        trajectoryLine.positionCount = trajectoryResolution;
    }

    private void StopAiming()
    {
        canThrow = false; // Blokuje rzut
        trajectoryLine.positionCount = 0; // Usuwa trajektoriê
    }

    private void HandleThrowInput()
    {
        if (canThrow)
        {
            ThrowStone();
            canThrow = false; // Zapobiega wielokrotnemu rzutowi
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

    private Vector3 GetAimPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            return hit.point;
        }
        return handPosition.position + transform.forward * 10f;
    }
}
