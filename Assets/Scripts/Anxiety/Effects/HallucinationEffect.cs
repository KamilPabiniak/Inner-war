using UnityEngine;

namespace Anxiety.Effects
{
    [CreateAssetMenu(menuName = "Anxiety/Effects/Hallucination Effect", fileName = "NewHallucinationEffect")]
    public class HallucinationEffect : BaseFearEffect
    {
       [SerializeField] private GameObject particle;
       private GameObject _currentParticle;
        protected override void ExecuteEffect()
        {
            _currentParticle = Instantiate(particle, Player.Instance.gameObject.transform);
        }

        protected override void EndEffect()
        {
            _currentParticle.GetComponent<ParticleSystem>().Stop();
        }
    }
}