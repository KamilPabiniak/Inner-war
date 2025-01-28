using Enemy.Type;
using UnityEngine;

namespace Enemy.Machine
{
    public class MachineDetection : MonoBehaviour
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

        [Header("References")]
        [SerializeField] private MachineEnemy mEnemy;
        public Light lightComponent;
        private bool _isPlayerInRange;
        private void Update()
        {
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, lightComponent.range, targetMask);
            Transform potentialTarget = targetsInRange.Length > 0 ? targetsInRange[0].transform : null;
            mEnemy.SetTarget(potentialTarget);

            if (mEnemy.Target)
            {
                UpdateDetection();
            }
            else
            {
                mEnemy.detectionProgress -= detectionDecreaseRate * Time.deltaTime;
            }

            lightComponent.color = mEnemy.CurrentState switch
            {
                PatrolState => Color.white,
                InvestigateState => Color.yellow,
                AttackState => Color.red,
                _ => lightComponent.color
            };
        }

        private void UpdateDetection()
        {
            float lightIntensity = CalculateLightIntensity(mEnemy.Target);
        
            if (lightIntensity > detectionThreshold)
            {
                mEnemy.seeTarget = true;
                mEnemy.detectionProgress += lightIntensity * detectionIncreaseRate * Time.deltaTime;
            }
            else
            {
                mEnemy.seeTarget = false;
                mEnemy.detectionProgress -= detectionDecreaseRate * Time.deltaTime;
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
}
