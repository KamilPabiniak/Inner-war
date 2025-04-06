using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }

    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private AudioSource soundGlobalFXObject;
    [SerializeField] private AudioSource soundGlobalNoEffectFXObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void PlaySoundFXClip(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource= Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    
    public void PlayGlobalSoundFXClip(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundGlobalFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    
    public void PlayGlobalSoundFXClipNoSfx(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundGlobalNoEffectFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    
    public void PlayGlobalSoundFXClipNoSfxDestroyOn(AudioClip clip, Transform spawnTransform, float volume, float destroyTime, bool onLoop)
    {
        AudioSource audioSource = Instantiate(soundGlobalNoEffectFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.SetParent(spawnTransform);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = onLoop;
        audioSource.Play();
        Destroy(audioSource.gameObject, destroyTime);
    }
}