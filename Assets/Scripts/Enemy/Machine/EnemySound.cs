using UnityEngine;
using UnityEngine.Serialization;

public class EnemySound : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private GameObject audioSources;

    [Header("Audio Clips - Footsteps")]
    [SerializeField] private AudioClip[] footStepClips;
    
    [Header("Audio Clips - State Sounds")]
    [SerializeField] private AudioClip patrolStateSound;
    [SerializeField] private AudioClip investigateStateSound;
    [SerializeField] private AudioClip attackStateSound;
    [SerializeField] private AudioClip targetLostSound;
    [SerializeField] private AudioClip overload;

    private bool footStepPlayed = false;
    
    public void ResetFootStepFlag() => footStepPlayed = false;

    public void PlayFootStepSound()
    {
        if (footStepClips == null || footStepPlayed) return;
        int rand = Random.Range(0, footStepClips.Length);
        SoundFXManager.Instance.PlaySoundFXClip(footStepClips[rand], audioSources.transform, 1f);
        footStepPlayed = true;
    }
    
    public void PlayPatrolSound() => PlayStateSound(patrolStateSound);

    public void PlayInvestigateSound() => PlayStateSound(investigateStateSound);

    public void PlayAttackSound() => PlayStateSound(attackStateSound);

    public void PlayTargetLostSound() => PlayStateSound(targetLostSound);
    public void PlayOverloadSound() => PlayStateSound(overload);

    private void PlayStateSound(AudioClip clip)
    {
        if (clip == null) return;
        SoundFXManager.Instance.PlaySoundFXClip(clip, audioSources.transform, 1f);
    }
}
