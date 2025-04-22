using System.Collections;
using UnityEngine;

public class EnemySound : MonoBehaviour
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
    [SerializeField] private float stateRange;

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
        PlayStateSound(patrolStateSound[random]);
    }

    public void PlayInvestigateSound()
    {
        int random = Random.Range(0, investigateStateSound.Length);
        PlayStateSound(investigateStateSound[random]);
    }
    
    public void PlayWarningSound()
    {
        PlayStateSound(warning);
    }

    public void PlayAttackSound()
    {
        int random = Random.Range(0, attackStateSound.Length);
        PlayStateSound(attackStateSound[random]);
    }

    public void PlayTargetLostSound()
    {
        int random = Random.Range(0, targetLostSound.Length);
        PlayStateSound(targetLostSound[random]);
    }

    public void PlayOverloadSound()
    {
        int random = Random.Range(0, overload.Length);
        PlayStateSound(overload[random]);
    }

    private void PlayStateSound(AudioClip clip)
    {
        if (clip == null) return;

        // Block if a state sound is still playing
        if (_currentStateAudio != null && _currentStateAudio.isPlaying)
            return;

        // Play new state sound and track its AudioSource
        _currentStateAudio = SoundFXManager.Instance.Play3DSoundFXClip(
            clip,
            audioSources.transform,
            1f,
            audioMixerGroup: SoundFXManager.Instance.LowPassMixer,
            maxDistance: stateRange
        );

        // Clear the reference after clip has finished
        if (_currentStateAudio != null)
            StartCoroutine(ClearStateSoundAfterDelay(clip.length));
    }

    private IEnumerator ClearStateSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentStateAudio = null;
    }
}
