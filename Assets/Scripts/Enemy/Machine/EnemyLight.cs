using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyLight : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetMask;
    public float viewRadius = 10f;
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
    [SerializeField] private Light lightComponent;
    private Transform _target;
    [SerializeField] private MachineEnemy mEnemy;
    private bool _isPlayerInRange;
    private float _detectionProgress = 0f;

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

        if (targetsInRange.Length > 0)
        {
            _target = targetsInRange[0].transform;
        }
        else
        {
            _target = null;
        }

        if (_target)
        {
            UpdateDetectionState();
        }
        else
        {
            if (_detectionProgress == 0) return;
            _detectionProgress -= detectionDecreaseRate * Time.deltaTime;
        }
    }

    private void UpdateDetectionState()
    {
        float lightIntensity = CalculateLightIntensity(_target);

        _detectionProgress = Mathf.Clamp(_detectionProgress, 0f, 100f);
        
        if (lightIntensity > detectionThreshold)
        {
            if (_detectionProgress == 100) return;
            _detectionProgress += lightIntensity * detectionIncreaseRate * Time.deltaTime;
        }
        else
        {
            if (_detectionProgress == 0) return;
            _detectionProgress -= detectionDecreaseRate * Time.deltaTime;
            mEnemy.seeTarget = false;
        }

        if (mEnemy.CurrentState is not AttackState)
        {
             if (_detectionProgress == 0) 
             {
                if (mEnemy.CurrentState is not PatrolState)
                {
                    mEnemy.ChangeState(new PatrolState());
                }
             }
            
             if (_detectionProgress >= 0.01f && _detectionProgress < 100f)
             {
                if (mEnemy.CurrentState is not InvestigateState)
                {
                    mEnemy.ChangeState(new InvestigateState(_target.transform.position, mEnemy));
                    mEnemy.seeTarget = true;
                    mEnemy.SetTarget(_target);
                }
             } 
        }
       
        
        if (_detectionProgress >= 100f)
        {
            if (mEnemy.CurrentState is not AttackState)
            {
                mEnemy.SetTarget(_target);
                mEnemy.ChangeState(new AttackState());
            }
        }

        UpdateLightAppearance();
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

    private void UpdateLightAppearance()
    {
        Color newColor = Color.white;

        if (_detectionProgress >= 50f && _detectionProgress < 100f)
        {
            newColor = Color.yellow;
        }
        else if (_detectionProgress >= 100f)
        {
            newColor = Color.red;
        }

        lightComponent.color = newColor;
    }

    private void DrawDebugRay(Vector3 origin, Vector3 direction, bool hit)
    {
        if (!debugRays) return;

        Color color = hit ? rayHitColor : rayMissColor;
        Debug.DrawLine(origin, origin + direction * 10f, color, 0.1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!debugFOV) return;
        Gizmos.color = _isPlayerInRange ? detectionColor : fovColor;
        Gizmos.DrawWireSphere(transform.position, lightComponent.range);
    }
}
