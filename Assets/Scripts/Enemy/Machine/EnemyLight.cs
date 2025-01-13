using System;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyLight : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetMask;
    [Tooltip("Prêdkoœæ wzrostu wykrycia.")]
    public float detectionIncreaseRate = 10f;
    [Tooltip("Prêdkoœæ spadku wykrycia.")]
    public float detectionDecreaseRate = 5f;
    [Tooltip("Minimalny próg wykrycia œwiat³a.")]
    public float detectionThreshold = 0.1f;
    [Tooltip("Mno¿nik efektu wykrywania w zale¿noœci od odleg³oœci.")]
    public AnimationCurve distanceEffectMultiplier = AnimationCurve.Linear(0, 1, 10, 0.1f);

    [Header("Debug Settings")]
    public bool debugFOV = true; 
    public Color fovColor = Color.green;
    public Color detectionColor = Color.red; 
    [Tooltip("Czy wyœwietlaæ debugowe linie Raycastów?")]
    public bool debugRays = true;
    [Tooltip("Kolor linii Raycastów trafiaj¹cych w gracza.")]
    public Color rayHitColor = Color.green;
    [Tooltip("Kolor linii Raycastów, które nie trafiaj¹ w gracza.")]
    public Color rayMissColor = Color.red;
    public bool debugConsole = true;

    [Header("References")]
    public Light lightComponent;
    [SerializeField] private MachineEnemy mEnemy;
    private Transform _target;
    private bool _isPlayerInRange;
    [Range(0f, 100f)]
    private float _detectionProgress;

    private void Awake()
    {
        EnemyLightManager.RegisterEnemyLight(lightComponent);
    }

    private void OnDestroy()
    {
        EnemyLightManager.UnregisterEnemyLight(lightComponent);
    }

    private void Update()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, lightComponent.range, targetMask);
        _target = targetsInRange.Length > 0 ? targetsInRange[0].transform : null;

        if (_target)
        {
            UpdateDetectionState();
        }
        else
        {
            _detectionProgress -= detectionDecreaseRate * Time.deltaTime;
        }

        switch (mEnemy.CurrentState)
        {
            case PatrolState:
                lightComponent.color = Color.white;
                break;
            case InvestigateState:
                lightComponent.color = Color.yellow;
                break;
            case AttackState:
                lightComponent.color = Color.red;
                break;
        }
    }

    private void UpdateDetectionState()
    {
        float lightIntensity = CalculateLightIntensity(_target);
        
        if (lightIntensity > detectionThreshold)
        {
            _detectionProgress += lightIntensity * detectionIncreaseRate * Time.deltaTime;
        }
        else
        {
            mEnemy.seeTarget = false;
            _detectionProgress -= detectionDecreaseRate * Time.deltaTime;
        }

        _detectionProgress = Mathf.Clamp(_detectionProgress, 0f, 100f);
        switch (_detectionProgress)
        {
            case  <= 4.00f:
            {
                if (mEnemy.CurrentState is not PatrolState)
                {
                    if (!mEnemy.CanChangeState) return;
                    lightComponent.color = Color.white;
                    mEnemy.ChangeState(new PatrolState());
                }

                break;
            }
            case > 4.0f and < 100f:
            {
                if (mEnemy.CurrentState is not InvestigateState)
                {
                    lightComponent.color = Color.yellow;
                    mEnemy.ChangeState(new InvestigateState(_target.transform.position, mEnemy));
                    mEnemy.seeTarget = true;
                    mEnemy.SetTarget(_target);
                    Debug.LogWarning($"[{mEnemy.name}] Rozpoczêto badanie pozycji celu: {_target.position}");
                }

                break;
            }
            case >= 100f:
            {
                if (mEnemy.CurrentState is AttackState) return;
                lightComponent.color = Color.red;
                mEnemy.SetTarget(_target);
                mEnemy.ChangeState(new AttackState());
                Debug.LogError($"[{mEnemy.name}] Cel wykryty w pe³ni! Rozpoczêto atak.");
                break;
            }
        }
    }

    private float CalculateLightIntensity(Transform target)
    {
        Vector3 directionToTarget = (target.position - lightComponent.transform.position).normalized;

        if (lightComponent.type == LightType.Spot)
        {
            float angle = Vector3.Angle(lightComponent.transform.forward, directionToTarget);
            if (angle > lightComponent.spotAngle / 2f)
            {
                DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
                return 0f;
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(lightComponent.transform.position, directionToTarget, out hit, lightComponent.range))
        {
            if (hit.transform != target)
            {
                DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
                return 0f;
            }

            DrawDebugRay(lightComponent.transform.position, directionToTarget, true);
        }
        else
        {
            DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
        }

        float distance = Vector3.Distance(lightComponent.transform.position, target.position);
        float distanceMultiplier = distanceEffectMultiplier.Evaluate(distance);
        float intensity = lightComponent.intensity / Mathf.Pow(distance, 2f);

        return intensity * distanceMultiplier;
    }
    
    private void DrawDebugRay(Vector3 origin, Vector3 direction, bool hit)
    {
        if (!debugRays) return;

        Color color = hit ? rayHitColor : rayMissColor;
        Debug.DrawLine(origin, origin + direction * lightComponent.range, color, 0.1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!debugFOV) return;
        Gizmos.color = _isPlayerInRange ? detectionColor : fovColor;
        Gizmos.DrawWireSphere(transform.position, lightComponent.range);
    }
}
