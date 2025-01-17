using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void ApplyAudioMuffle()
    {
        audioMixer.SetFloat("cutoffFreq", 350f);
    }

    public void ResetAudioEffects()
    {
        audioMixer.SetFloat("cutoffFreq", 22000f);
    }
}
