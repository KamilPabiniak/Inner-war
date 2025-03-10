using UnityEngine;
using Random = UnityEngine.Random;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Death Door Effect", fileName = "NewDeathDoorEffect")]
    public class DeathDoorEffect : BaseFearEffect
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float chanceToKill = 0.3f;
        [SerializeField]
        private float addDeathChancesEverySuccess = 0f;
        
        private float _defaultChanceToKill;
        private void Awake()
        {
            _defaultChanceToKill = chanceToKill;
        }

        private void OnDisable()
        {
            _defaultChanceToKill = chanceToKill;
        }

        protected override void ExecuteEffect()
        {
            float roll = Random.value;
            if (roll <= chanceToKill)
            {
                Player.Instance.GetModule<PlayerDeath>().Kill();
                chanceToKill = _defaultChanceToKill;
            }
            else
            {
                chanceToKill += addDeathChancesEverySuccess;
            }
        }
    }
}