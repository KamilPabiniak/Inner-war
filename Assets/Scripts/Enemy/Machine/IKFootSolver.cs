using UnityEngine;

namespace Enemy.Machine
{
    public class IKFootSolver : MonoBehaviour
    {
        [Header("Main")] [Range(0, 1)] public float weight = 1f;
        [Header("Settings")] public float maxStep = 0.5f;
        public float footRadius = 0.15f;
        public LayerMask ground = 1;
        public float offset;
        [Header("Speed")] public float hipsPositionSpeed = 1f;
        public float feetPositionSpeed = 2f;
        public float feetRotationSpeed = 90;
        [Header("Weight")] [Range(0, 1)] public float hipsWeight = 0.75f;
        [Range(0, 1)] public float footPositionWeight = 1f;
        [Range(0, 1)] public float footRotationWeight = 1f;
        [Header("Footstep Detection")] public float stepThreshold = 0.1f;

        [SerializeField] private Animator anim;
        [SerializeField] private EnemyAudio enemyAudio;
        [Header("Debug")] public bool showDebug = true;

        // Detection state
        private bool _leftFootWasGrounded;
        private bool _rightFootWasGrounded;

        // IK & grounding
        private Vector3 _likPosition, _rikPosition;
        private Vector3 _lNormal, _rNormal;
        private Quaternion _likRotation, _rikRotation;

        private float _velocity;
        private float _falloffWeight;
        private float _lastHeight;
        private Vector3 _lastPosition;
        private bool _lGrounded, _rGrounded;

        private void FixedUpdate()
        {
            if (weight == 0 || anim == null) return;

            // Update velocity for lerp
            Vector3 speed = (_lastPosition - anim.transform.position) / Time.fixedDeltaTime;
            _velocity = Mathf.Clamp(speed.magnitude, 1f, speed.magnitude);
            _lastPosition = anim.transform.position;

            // Raycast to find foot targets
            FeetSolver(HumanBodyBones.LeftFoot,  ref _likPosition, ref _lNormal, ref _likRotation, ref _lGrounded);
            FeetSolver(HumanBodyBones.RightFoot, ref _rikPosition, ref _rNormal, ref _rikRotation, ref _rGrounded);

            // Compute falloff for IK weights
            _falloffWeight = LerpValue(_falloffWeight, (_lGrounded || _rGrounded) ? 1f : 0f,
                                       1f, 10f, Time.fixedDeltaTime) * weight;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (weight == 0 || anim == null) return;

            // --- Apply IK ---
            MovePelvisHeight();
            MoveIK(AvatarIKGoal.LeftFoot,  _likPosition, _lNormal, _likRotation,  ref _lastHeight, ref _likRotation);
            MoveIK(AvatarIKGoal.RightFoot, _rikPosition, _rNormal, _rikRotation, ref _lastHeight, ref _rikRotation);

            // --- Footstep detection based on IK gap ---
            float leftIKY  = anim.GetIKPosition(AvatarIKGoal.LeftFoot).y;
            float rightIKY = anim.GetIKPosition(AvatarIKGoal.RightFoot).y;

            float leftGap  = leftIKY  - _likPosition.y;
            float rightGap = rightIKY - _rikPosition.y;

            bool leftGroundedNow  = leftGap  < stepThreshold;
            bool rightGroundedNow = rightGap < stepThreshold;

            if (showDebug)
            {
                Debug.Log($"[IK] LeftGap={leftGap:F3} (groundedNow={leftGroundedNow})");
                Debug.Log($"[IK] RightGap={rightGap:F3} (groundedNow={rightGroundedNow})");
            }

            // LEFT foot
            if (_leftFootWasGrounded && !leftGroundedNow)
            {
                enemyAudio.ResetFootStepFlag();
                if (showDebug) Debug.Log("Left foot lifted");
            }
            if (!_leftFootWasGrounded && leftGroundedNow)
            {
                enemyAudio.PlayFootStepSound();
                if (showDebug) Debug.Log("Left foot landed");
            }
            _leftFootWasGrounded = leftGroundedNow;

            // RIGHT foot
            if (_rightFootWasGrounded && !rightGroundedNow)
            {
                enemyAudio.ResetFootStepFlag();
                if (showDebug) Debug.Log("Right foot lifted");
            }
            if (!_rightFootWasGrounded && rightGroundedNow)
            {
                enemyAudio.PlayFootStepSound();
                if (showDebug) Debug.Log("Right foot landed");
            }
            _rightFootWasGrounded = rightGroundedNow;
        }

        private void MovePelvisHeight()
        {
            float leftOffset  = _likPosition.y - anim.transform.position.y;
            float rightOffset = _rikPosition.y - anim.transform.position.y;
            float lowest = Mathf.Min(leftOffset, rightOffset);

            Vector3 newPos = anim.bodyPosition;
            float targetHeight = lowest * (hipsWeight * _falloffWeight);
            _lastHeight = Mathf.MoveTowards(_lastHeight, targetHeight, hipsPositionSpeed * Time.deltaTime);
            newPos.y += _lastHeight + offset;

            anim.bodyPosition = newPos;
        }

        private void MoveIK(AvatarIKGoal foot, Vector3 ikPos, Vector3 normal, Quaternion ikRot,
                            ref float lastHeight, ref Quaternion lastRot)
        {
            Vector3 footPos = anim.GetIKPosition(foot);
            Quaternion footRot = anim.GetIKRotation(foot);

            footPos = anim.transform.InverseTransformPoint(footPos);
            Vector3 target = anim.transform.InverseTransformPoint(ikPos);

            lastHeight = Mathf.MoveTowards(lastHeight, target.y, feetPositionSpeed * Time.deltaTime);
            footPos.y += lastHeight;
            footPos = anim.transform.TransformPoint(footPos);
            footPos += normal * offset;

            Quaternion relative = Quaternion.Inverse(ikRot * footRot) * footRot;
            lastRot = Quaternion.RotateTowards(lastRot, Quaternion.Inverse(relative), feetRotationSpeed * Time.deltaTime);
            footRot *= lastRot;

            anim.SetIKPosition(foot, footPos);
            anim.SetIKRotation(foot, footRot);
            anim.SetIKPositionWeight(foot, footPositionWeight * _falloffWeight);
            anim.SetIKRotationWeight(foot, footRotationWeight * _falloffWeight);
        }

        private void FeetSolver(HumanBodyBones foot, ref Vector3 ikPos, ref Vector3 normal,
                                 ref Quaternion ikRot, ref bool grounded)
        {
            Vector3 origin = anim.GetBoneTransform(foot).position;
            origin.y = anim.transform.position.y + maxStep;

            if (showDebug)
                Debug.DrawLine(origin, origin + Vector3.down * (maxStep * 2), Color.yellow);

            if (Physics.SphereCast(origin, footRadius, Vector3.down, out RaycastHit hit,
                                   maxStep * 2, ground))
            {
                ikPos = hit.point;
                normal = hit.normal;
                Vector3 axis = Vector3.Cross(Vector3.up, hit.normal);
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                ikRot = Quaternion.AngleAxis(angle, axis);
                grounded = true;
                return;
            }

            grounded = false;
            ikPos = origin + Vector3.down * maxStep;
            ikRot = Quaternion.identity;
        }

        private float LerpValue(float current, float desired,
                                float incSpeed, float decSpeed, float dt)
        {
            if (current == desired) return desired;
            float speed = (current < desired ? incSpeed : decSpeed) * _velocity;
            return Mathf.MoveTowards(current, desired, speed * dt);
        }
    }
}
