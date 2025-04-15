using System;
using UnityEngine;

public class Ambience : MonoBehaviour
{
   private AudioSource _ambience;

   private void OnEnable()
   {
      GameEvents.onPlayerDied += StopAmbience;
      GameEvents.onPlayerRespawned += PlayAmbience;
   }

   private void OnDisable()
   {
      GameEvents.onPlayerDied -= StopAmbience;
      GameEvents.onPlayerRespawned -= PlayAmbience;
   }

   private void Start()
   {
      _ambience = GetComponent<AudioSource>();
   }

   private void PlayAmbience()
   {
      _ambience.Play();
   }
   
   private void StopAmbience()
   {
      _ambience.Stop();
   }
}
