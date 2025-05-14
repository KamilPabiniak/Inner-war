using UnityEngine;
using System;

namespace Enemy 
{
    public class DetectionController : MonoBehaviour 
    {
        [Header("Reference")]
        public Light lightComponent;
        public SpriteRenderer detectionMark;
        [Header("Detection")]
        public LayerMask targetMask;
        [Range(0f,1f)] public float threshold = 0.1f;
        public float increaseRate = 10f, decreaseRate = 5f;
        public AnimationCurve distanceCurve = AnimationCurve.Linear(0,1,10,0.1f);

        public event Action<Transform> OnSpotted;
        public event Action<float> OnProgress;

        private float _awarenessLevel;
        private Transform _target;
        private bool _isPlayerVisible;
        public float AwarenessLevel => _awarenessLevel;
        public bool IsPlayerVisible => _isPlayerVisible;

        void Update() {
            Sense();
            UpdateProgress();
        }

        public void SetAwarenessLevel(float value) => _awarenessLevel = value;

        private void Sense()
        {
            Transform found = null;
            Vector3 origin = lightComponent.transform.position;
            var hits = Physics.OverlapSphere(origin, lightComponent.range, targetMask);
            foreach (var c in hits)
            {
                if (!c.CompareTag("Player")) continue;

                var dir = (c.transform.position - origin).normalized;
                
                if (lightComponent.type == LightType.Spot &&
                    Vector3.Angle(lightComponent.transform.forward, dir) > lightComponent.spotAngle * 0.5f)
                {
                    continue;
                }

                if (Physics.Raycast(origin, dir, out var hit, lightComponent.range) &&
                    hit.transform == c.transform)
                {
                    found = c.transform;
                    break;
                }
            }

            if (found != _target)
            {
                _target = found;
                if (_target != null)
                    OnSpotted?.Invoke(_target);
            }

            _isPlayerVisible = (_target != null);
        }


        void UpdateProgress() {
            float intensity = _isPlayerVisible ? CalcIntensity(_target) : 0f;
            
            if (intensity > threshold) {
                _awarenessLevel += intensity * increaseRate * Time.deltaTime;
            } else {
                _awarenessLevel -= decreaseRate * Time.deltaTime;
            }
            
            _awarenessLevel = Mathf.Clamp(_awarenessLevel, 0f, 100f);
            OnProgress?.Invoke(_awarenessLevel);
        }

        float CalcIntensity(Transform t) {
            var origin = lightComponent.transform.position;
            var dir = (t.position - origin).normalized;
            if (lightComponent.type == LightType.Spot &&
                Vector3.Angle(lightComponent.transform.forward, dir) > lightComponent.spotAngle*0.5f)
                return 0f;
            if (Physics.Raycast(origin, dir, out var hit, lightComponent.range) && hit.transform==t)
                return (lightComponent.intensity/(hit.distance*hit.distance))
                       * distanceCurve.Evaluate(hit.distance);
            return 0f;
        }
    }
}
