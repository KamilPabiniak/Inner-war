using System.Collections;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private GameObject audioSources;

    [Header("Audio Clips - Footsteps")]
    [SerializeField] private AudioClip[] footStepClips;
    [SerializeField] private float footStepsRange;
    
    [Header("Audio Clips - State Sounds")]
    [SerializeField] private AudioClip[] patrolStateSound;
    [SerializeField] private AudioClip[] investigateStateSound;
    [SerializeField] private AudioClip[] attackStateSound;
    [SerializeField] private AudioClip[] targetLostSound;
    [SerializeField] private AudioClip[] overload;
    [SerializeField] private AudioClip warning;
    [SerializeField] private AudioClip targetEscaped;
    [SerializeField] private float stateRange;
    [SerializeField] private float stateVolume;

    private bool _footStepPlayed;
    private AudioSource _currentStateAudio;
    
    public void ResetFootStepFlag() => _footStepPlayed = false;

    public void PlayFootStepSound()
    {
        if (footStepClips == null || _footStepPlayed) return;
        int rand = Random.Range(0, footStepClips.Length);
        SoundFXManager.Instance.Play3DSoundFXClip(footStepClips[rand], audioSources.transform, 1f, audioMixerGroup: SoundFXManager.Instance.LowPassMixer, maxDistance:footStepsRange);
        _footStepPlayed = true;
    }
    
    public void PlayPatrolSound()
    {
        int random = Random.Range(0, patrolStateSound.Length);
        PlayMachineVoice(patrolStateSound[random]);
    }

    public void PlayInvestigateSound()
    {
        int random = Random.Range(0, investigateStateSound.Length);
        PlayMachineVoice(investigateStateSound[random]);
    }
    
    public void PlayWarningSound()
    {
        PlayMachineVoice(warning);
    }
    public void PlayTargetEscapeSound()
    {
        PlayMachineVoice(targetEscaped);
    }
    public void PlayAttackSound()
    {
        int random = Random.Range(0, attackStateSound.Length);
        PlayMachineVoice(attackStateSound[random]);
    }

    public void PlayTargetLostSound()
    {
        int random = Random.Range(0, targetLostSound.Length);
        PlayMachineVoice(targetLostSound[random]);
    }

    public void PlayOverloadSound()
    {
        int random = Random.Range(0, overload.Length);
        PlayMachineVoice(overload[random]);
    }

    private void PlayMachineVoice(AudioClip clip)
    {
        if (clip == null) return;
        
        if (_currentStateAudio != null && _currentStateAudio.isPlaying)
            return;

        _currentStateAudio = SoundFXManager.Instance.Play3DSoundFXClip(
            clip,
            audioSources.transform,
            stateVolume,
            audioMixerGroup: SoundFXManager.Instance.MachineState,
            maxDistance: stateRange
        );
        
        if (_currentStateAudio != null)
            StartCoroutine(ClearStateSoundAfterDelay(clip.length));
    }


    private IEnumerator ClearStateSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentStateAudio = null;
    }
}
