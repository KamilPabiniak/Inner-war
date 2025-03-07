using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/HeartbeatPlay", fileName = "NewHeartbeatPlay")]
    public class HeartEffect : BaseFearEffect
    {
        [Header("Heart Settings")]
        [SerializeField] private float heartFadeInDuration = 1f;
        [SerializeField] private float heartFadeOutDuration = 1f;
        [SerializeField] private float heartTargetVolume = 1f;

        // drymixEcho values: by default off = 0f, on = 60f (can be modified via the Inspector)
        [Tooltip("If u want faster Heartbeat use heartDrymixEchoOnValue on 60")]
        [Range(0f, 60f)]
        [SerializeField] private float heartDrymixEchoOnValue = 60f;
        [SerializeField] private float heartDrymixEchoOffValue = 0f;
        protected override void ExecuteEffect()
        {
            AnxietyManager.Instance.audioEffectsController.PlayHeart(heartDrymixEchoOnValue, heartFadeInDuration, heartTargetVolume);
        }

        protected override void EndEffect()
        {
            AnxietyManager.Instance.audioEffectsController.StopHeart(heartFadeOutDuration);
        }
    }
}