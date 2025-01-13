using UnityEngine;
using UnityEngine.Serialization;

public class EnemySoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource footStepLeftSource;
    [SerializeField] private AudioSource footStepRightSource;
    [SerializeField] private AudioSource detectionSource;
    [SerializeField] private AudioSource stateSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip footStepClip;
    [SerializeField] private AudioClip lightDetectionWhiteClip;
    [SerializeField] private AudioClip lightDetectionYellowClip;
    [SerializeField] private AudioClip lightDetectionRedClip;
    [Header("Audio Clips - State Sounds")]
    [SerializeField] private AudioClip patrolStateSound;
    [SerializeField] private AudioClip investigateStateSound;
    [SerializeField] private AudioClip attackStateSound;
    [SerializeField] private AudioClip targetLostSound;
    [SerializeField] private AudioClip overload;

    private EnemyLight enemyLight;
    private EnemyBase enemyBase;
    private bool _playLeftFootNext = true;
    private bool footStepPlayed = false;
    

    private void Awake()
    {
        enemyLight = GetComponent<EnemyLight>();
        enemyBase = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        //HandleLightDetectionSounds();
    }

    public void ResetFootStepFlag() => footStepPlayed = false;

    public void PlayFootStepSound()
    {
        if (footStepClip == null || footStepPlayed) return;
        switch (_playLeftFootNext)
        {
            case true when !footStepLeftSource.isPlaying:
                footStepLeftSource.PlayOneShot(footStepClip);
                break;
            case false when !footStepRightSource.isPlaying:
                footStepRightSource.PlayOneShot(footStepClip);
                break;
        }
        _playLeftFootNext = !_playLeftFootNext;
        footStepPlayed = true;
    }

    private void HandleLightDetectionSounds()
    {
        Color currentColor = enemyLight.lightComponent.color;

        if (currentColor.Equals(Color.white))
        {
            PlayDetectionSound(lightDetectionWhiteClip);
        }
        else if (currentColor.Equals(Color.yellow))
        {
            PlayDetectionSound(lightDetectionYellowClip);
        }
        else if (currentColor.Equals(Color.red))
        {
            PlayDetectionSound(lightDetectionRedClip);
        }
    }
    
    private void PlayDetectionSound(AudioClip clip)
    {
        if (detectionSource.clip != clip || !detectionSource.isPlaying)
        {
            detectionSource.clip = clip;
            detectionSource.Play();
        }
    }
    
    public void PlayPatrolSound() => PlayStateSound(patrolStateSound);

    public void PlayInvestigateSound() => PlayStateSound(investigateStateSound);

    public void PlayAttackSound() => PlayStateSound(attackStateSound);

    public void PlayTargetLostSound() => PlayStateSound(targetLostSound);
    public void PlayOverloadSound() => PlayStateSound(overload);

    private void PlayStateSound(AudioClip clip)
    {
        if (clip == null || stateSource.isPlaying) return;
        stateSource.clip = clip;
        stateSource.Play();
    }
}
