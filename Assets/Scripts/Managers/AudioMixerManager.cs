using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Managers
{
    public class AudioMixerManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;

        private const string SfxKey = "SFXVolume";
        private const string MusicKey = "MusicVolume";
        
        private float _currentSfxVolume = 0.5f;
        private float _currentMusicVolume = 0.5f;

        private void Start()
        {
            LoadVolumeSettings();
            InitializeSliders();
        }
        
        private void InitializeSliders()
        {
            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat(SfxKey, 0.5f);
                sfxSlider.onValueChanged.AddListener(SetSoundFXVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 0.5f);
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
        }

        public void SetSoundFXVolume(float level)
        {
            _currentSfxVolume = level;
            float dbVolume = level > 0.001f ? Mathf.Log10(level) * 20f : -80f;
            audioMixer.SetFloat("volumeSFX", dbVolume);
            SaveVolumeSettings();
        }
    
        public void SetMusicVolume(float level)
        {
            _currentMusicVolume = level;
            float dbVolume = level > 0.001f ? Mathf.Log10(level) * 20f : -80f;
            audioMixer.SetFloat("volumeMusic", dbVolume);
            SaveVolumeSettings();
        }
    
        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(SfxKey, _currentSfxVolume);
            PlayerPrefs.SetFloat(MusicKey, _currentMusicVolume);
            PlayerPrefs.Save();
        }
    
        private void LoadVolumeSettings()
        {
            _currentSfxVolume = PlayerPrefs.GetFloat(SfxKey, _currentSfxVolume);
            _currentMusicVolume = PlayerPrefs.GetFloat(MusicKey, _currentMusicVolume);
            float sfxDb = _currentSfxVolume > 0.001f ? Mathf.Log10(_currentSfxVolume) * 20f : -80f;
            float musicDb = _currentMusicVolume > 0.001f ? Mathf.Log10(_currentMusicVolume) * 20f : -80f;
            audioMixer.SetFloat("volumeSFX", sfxDb);
            audioMixer.SetFloat("volumeMusic", musicDb);
        }
    }
}
