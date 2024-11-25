using UnityEngine;
using System.Collections.Generic;

public class PlayerFootstepSounds : PlayerModule
{
    [Header("Audio Settings")]
    public float footstepInterval = 0.5f;

    [System.Serializable]
    public class SurfaceSounds
    {
        [SerializeField] private string surfaceName;
        public LayerMask surfaceLayer;
        public List<AudioClip> footstepSounds;
    }

    public List<SurfaceSounds> surfaceSoundMappings = new List<SurfaceSounds>();
    [SerializeField] private AudioSource audioSource;
    private Transform footPosition;
    private PlayerMovement _movement;
    private float nextFootstepTime;

    private void Start()
    {
        footPosition = transform;
        _movement = GetComponent<PlayerMovement>();
        nextFootstepTime = Time.time;
    }

    private void Update()
    {
        if (_movement.currentVelocity.magnitude > 0.1f && Time.time >= nextFootstepTime)
        {
            PlayFootstepSound();
            nextFootstepTime = Time.time + footstepInterval;
        }
    }

    private void PlayFootstepSound()
    {
        if (!Physics.Raycast(footPosition.position, Vector3.down, out RaycastHit hit, 1f)) return;
        foreach (var surface in surfaceSoundMappings)
        {
            if ((surface.surfaceLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                if (surface.footstepSounds.Count > 0)
                {
                    AudioClip clip = surface.footstepSounds[Random.Range(0, surface.footstepSounds.Count)];
                    audioSource.PlayOneShot(clip);
                }
                return;
            }
        }
    }
}
