using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Shake Effect", fileName = "NewShakeEffect")]
    public class ShakeEffect : BaseFearEffect
    {
        private PlayerLook playerLook;
        [Header("Camera Shake Settings")]
        [Tooltip("Amplitude of the camera shake effect")]
        public float shakeAmplitude = 0.3f;
        [Tooltip("Frequency of the camera shake effect")]
        public float shakeFrequency = 20f;
        [Tooltip("Falloff curve for the shake effect")]
        public AnimationCurve shakeFalloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [Tooltip("Should the shake affect the camera's position?")]
        public bool shakePosition = true;
        [Tooltip("Should the shake affect the camera's rotation?")]
        public bool shakeRotation = true;
        [Tooltip("Camera shake type:\n• Perlin: Smooth shake using Perlin Noise.\n• Random: Shake using random values.\n• Directional: Shake in a fixed direction (modulated by a sine wave) using the 'shakeDirection' parameter.")]
        public CameraShakeType shakeType = CameraShakeType.Perlin;
        [Tooltip("Specified shake direction (used in Directional mode)")]
        public Vector3 shakeDirection = Vector3.one;

        protected override void ExecuteEffect()
        {
            playerLook = Player.Instance.GetModule<PlayerLook>();
            playerLook.StartCameraShake(currentDuration, shakeAmplitude, shakeFrequency, shakeFalloffCurve, shakePosition, shakeRotation, shakeType, shakeDirection);
        }
    }
}