using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class PlayerFootstepSounds : PlayerModule
{
    [Header("Audio Settings")]
    public float footstepInterval = 0.5f;
    public GameObject footstepSource;

    [Serializable]
    public class SurfaceSounds
    {
        public LayerMask surfaceLayer;
        public List<AudioClip> footstepSounds;
    }

    public List<SurfaceSounds> surfaceSoundMappings = new List<SurfaceSounds>();
    private Transform _footPosition;
    private PlayerMovement _movement;
    private float _nextFootstepTime;

    private void Start()
    {
        _footPosition = transform;
        _movement = GetComponent<PlayerMovement>();
        _nextFootstepTime = Time.time;
    }

    private void Update()
    {
        if (!(_movement.currentVelocity.magnitude > 0.1f) || !(Time.time >= _nextFootstepTime)) return;
        PlayFootstepSound();
        _nextFootstepTime = Time.time + footstepInterval;
    }

    private void PlayFootstepSound()
    {
        if (!Physics.Raycast(_footPosition.position, Vector3.down, out RaycastHit hit, 1f)) return;
        foreach (var surface in surfaceSoundMappings)
        {
            if ((surface.surfaceLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                if (surface.footstepSounds.Count > 0)
                {
                    AudioClip clip = surface.footstepSounds[Random.Range(0, surface.footstepSounds.Count)];
                    SoundFXManager.Instance.Play3DSoundFXClip(clip, footstepSource.transform, 1f, audioMixerGroup: SoundFXManager.Instance.LowPassMixer);
                }
                return;
            }
        }
    }
}
