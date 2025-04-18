using Enemy.State;
using UnityEngine;

namespace Enemy.Type
{
    public class MachineEnemy : EnemyBase
    {
        [Binder("Machine Specification", fontSize: 16, bottomSpace: 5f, topSpace: 40f, alignment: TextAnchor.MiddleLeft, fontStyle: FontStyle.Bold, foldAll: true, colorHex:"#a1adff")]
        
        [Binder("Reference", bottomSpace: 1f, alignment: TextAnchor.MiddleLeft , fontStyle: FontStyle.BoldAndItalic, colorHex: "#ffeca1")]
        public GameObject sightTarget;
        public Vector3 OriginalHeadPos { get; private set; }
        public Light lightComponent;
        public LayerMask targetMask;
        [Tooltip("Prêdkoœæ wzrostu wykrycia.")]
        public float detectionIncreaseRate = 10f;
        [Tooltip("Prêdkoœæ spadku wykrycia.")]
        public float detectionDecreaseRate = 5f;
        [Tooltip("Minimalny próg wykrycia œwiat³a.")]
        public float detectionThreshold = 0.1f;
        [Tooltip("Mno¿nik efektu wykrywania w zale¿noœci od odleg³oœci.")]
        public AnimationCurve distanceEffectMultiplier = AnimationCurve.Linear(0, 1, 10, 0.1f);
        
        private bool _isPlayerInRange;
        
        [Header("Patrol Head Specification")]
        public float headRotationSpeed;
        public int stopPoints;
        public float stopDuration = 0.5f;
        [Range(1,5)]
        [Tooltip("Max distance from SightTarget")]
        public int maxOffsetDistance = 3;
        public float headDetectionDistance = 5f;
        
        [Header("Raycast Settings")]
        public float mainDetectionDistance = 6f;
        public float sideDetectionDistance = 4f;
        public float rayOriginHeight = 1.5f;
        public float sideRayAngleOffset = 30f;
        public float additionalRaycastAngleOffset = 20f;
        
        [Header("Debug Settings")]
        [Tooltip("Draw the field?of?view sphere")]
        public bool debugFOV = true; 
        [Tooltip("Draw debug rays for wall detection in editor")]
        public bool debugWallRays = true;
        public Color fovColor = Color.green;
        public Color detectionColor = Color.red; 
        [Tooltip("Czy wyœwietlaæ debugowe linie Raycastów?")]
        public bool debugRays = true;
        [Tooltip("Kolor linii Raycastów trafiaj¹cych w gracza.")]
        public Color rayHitColor = Color.green;
        [Tooltip("Kolor linii Raycastów, które nie trafiaj¹ w gracza.")]
        public Color rayMissColor = Color.red;

      
        
        private void Start()
        {
            OriginalHeadPos = sightTarget.transform.localPosition;
            ChangeState(new PatrolState());
        }

        private void FixedUpdate()
        {
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, lightComponent.range, targetMask);
            Transform potentialTarget = targetsInRange.Length > 0 ? targetsInRange[0].transform : null;
            SetTarget(potentialTarget);

            if (Target)
            {
                UpdateDetection();
            }
            else
            {
                detectionProgress -= detectionDecreaseRate * Time.deltaTime;
            }

            lightComponent.color = CurrentState switch
            {
                PatrolState => Color.white,
                InvestigateState => Color.yellow,
                AttackState => Color.red,
                _ => lightComponent.color
            };
            UpdateDetectionState();
        }

        private void UpdateDetectionState()
        {
            detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);

            if (detectionProgress <= 0.1f && CurrentState is not PatrolState)
            {
                if (!CanChangeState) return;
                ChangeState(new PatrolState());
            }
            else if (detectionProgress > 0.1f && detectionProgress < 100f && CurrentState is not InvestigateState)
            {
                if (!CanChangeState) return;
                ChangeState(new InvestigateState(Target != null ? Target.position : transform.position));
            }
            else if (detectionProgress >= 100f && CurrentState is not AttackState)
            {
                if (!IsTargetInNavMesh(out _)) return;
                ChangeState(new AttackState());
            }
        }
        
        private void UpdateDetection()
        {
            float lightIntensity = CalculateLightIntensity(Target);
        
            if (lightIntensity > detectionThreshold)
            {
                seeTarget = true;
                detectionProgress += lightIntensity * detectionIncreaseRate * Time.deltaTime;
            }
            else
            {
                seeTarget = false;
                detectionProgress -= detectionDecreaseRate * Time.deltaTime;
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

        [ContextMenu("Patrol")]
        public void ForcePatrol()
        {
            ChangeState(new PatrolState());
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugFOV) return;
            // Draw FOV sphere
            Gizmos.color = _isPlayerInRange ? detectionColor : fovColor;
            Gizmos.DrawWireSphere(transform.position, lightComponent.range);

            if (!debugWallRays) return;
            // Draw wall detection rays
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            
            // Main forward ray
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, transform.forward * mainDetectionDistance);

            // Side rays
            Gizmos.color = Color.cyan;
            Vector3 leftDir  = Quaternion.Euler(0, -sideRayAngleOffset, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0,  sideRayAngleOffset, 0) * transform.forward;
            Gizmos.DrawRay(origin, leftDir * sideDetectionDistance);
            Gizmos.DrawRay(origin, rightDir * sideDetectionDistance);

            // Additional machine-specific rays
            Gizmos.color = Color.magenta;
            Vector3 extraL = Quaternion.Euler(0, -additionalRaycastAngleOffset, 0) * transform.forward;
            Vector3 extraR = Quaternion.Euler(0,  additionalRaycastAngleOffset, 0) * transform.forward;
            Gizmos.DrawRay(origin, extraL * mainDetectionDistance);
            Gizmos.DrawRay(origin, extraR * mainDetectionDistance);
        }

    }
}

