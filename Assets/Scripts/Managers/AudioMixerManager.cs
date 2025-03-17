using UnityEngine;
using UnityEngine.Audio;

namespace Managers
{
    public class AudioMixerManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        public void SetSoundFXVolume(float level)
        {
            audioMixer.SetFloat("volumeSFX", Mathf.Log10(level) * 20f);
        }
    
        public void SetMusicVolume(float level)
        {
            audioMixer.SetFloat("volumeMusic", Mathf.Log10(level) * 20f);
        }
    }
}
