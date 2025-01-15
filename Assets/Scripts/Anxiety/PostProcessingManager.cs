using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    [Header("Post Processing Profiles")]
    public VolumeProfile slightFearProfile;
    public VolumeProfile moderateFearProfile;
    public VolumeProfile highFearProfile;
    public VolumeProfile extremeFearProfile;
    public VolumeProfile faintProfile;

    [SerializeField] private Volume _volume;

    public void UpdatePostProcessingProfile(float fearLevel)
    {
        VolumeProfile selectedProfile = null;

        if (fearLevel <= 20) return; 
        else if (fearLevel <= 40)
            selectedProfile = slightFearProfile;
        else if (fearLevel <= 60)
            selectedProfile = moderateFearProfile;
        else if (fearLevel <= 80)
            selectedProfile = highFearProfile;
        else if (fearLevel <= 99)
            selectedProfile = extremeFearProfile;
        else
            selectedProfile = faintProfile;

        if (selectedProfile != null && _volume.profile != selectedProfile)
        {
            _volume.profile = selectedProfile;
        }
    }
}