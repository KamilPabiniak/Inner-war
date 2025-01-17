using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private VolumeProfile slight;
    [SerializeField] private VolumeProfile moderate;
    [SerializeField] private Volume volume;

    public void ApplyEdgeBlur()
    {
        volume.weight = 1;
    }

    public void ResetEffects()
    {
        volume.weight = 0;
    }

    public void UpdatePostProcessingProfile(float fearLevel)
    {
        VolumeProfile selectedProfile = null;

        if (fearLevel <= 20) return;
        else if (fearLevel <= 40)
            selectedProfile = slight;
        else if (fearLevel <= 60)
            selectedProfile = moderate;
        else if (fearLevel <= 80)
            selectedProfile = null;
        else if (fearLevel <= 99)
            selectedProfile = null;
        else
            selectedProfile = null;

        if (selectedProfile != null && volume.profile != selectedProfile)
        {
            volume.profile = selectedProfile;
        }
    }
}
