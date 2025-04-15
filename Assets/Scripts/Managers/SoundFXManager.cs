using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource3D;
    [SerializeField] private AudioSource audioSource2D;
    
    [Header("Audio Mixers")]
    [SerializeField] private AudioMixerGroup lowPassMixer;
    [SerializeField] private AudioMixerGroup voicesMixer;
    [SerializeField] private AudioMixerGroup heartMixer;

    public AudioMixerGroup LowPassMixer => lowPassMixer;
    public AudioMixerGroup VoicesMixer => voicesMixer;
    public AudioMixerGroup HeartMixer => heartMixer;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public AudioSource Play3DSoundFXClip(AudioClip clip, Transform spawnTransform, float volume, float maxDistance = 10f, AudioMixerGroup audioMixerGroup = null)
    {
        AudioSource audioSource= Instantiate(audioSource3D, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.maxDistance = maxDistance;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
        return audioSource;
    }
    
    public AudioSource Play2DSoundFXClip(AudioClip clip, Transform spawnTransform, float volume, AudioMixerGroup audioMixerGroup = null)
    {
        AudioSource audioSource = Instantiate(audioSource2D, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
        return audioSource;
    }
    
    public AudioSource Play2DSoundFXClipDestroyOn(AudioClip clip, Transform spawnTransform, float volume, float destroyTime, bool onLoop, AudioMixerGroup audioMixerGroup = null)
    {
        AudioSource audioSource = Instantiate(audioSource2D, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = onLoop;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();
        Destroy(audioSource.gameObject, destroyTime);
        return audioSource;
    }
}