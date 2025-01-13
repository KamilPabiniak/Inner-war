using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MachineSoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource footStepLeftSource;
    [SerializeField] private AudioSource footStepRightSource;
    [SerializeField] private AudioSource detectionSource;
    [SerializeField] private AudioSource stateChangeSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip footStepClip;
    [SerializeField] private AudioClip lightDetectionWhiteClip;
    [SerializeField] private AudioClip lightDetectionYellowClip;
    [SerializeField] private AudioClip lightDetectionRedClip;
    [SerializeField] private AudioClip stateChangeClip;

    private EnemyLight enemyLight;
    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyLight = GetComponent<EnemyLight>();
        enemyBase = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        //HandleLightDetectionSounds();
    }

    public void PlayFootStepLeftSound()
    {
        if (footStepClip != null)
        {
            footStepLeftSource.PlayOneShot(footStepClip);
        }
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

    public void PlayStateChangeSound()
    {
        stateChangeSource.clip = stateChangeClip;
        stateChangeSource.Play();
    }
}
