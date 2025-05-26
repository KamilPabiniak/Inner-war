using System.Collections.Generic;
using Anxiety.Effects;
using UnityEngine;

[CreateAssetMenu(menuName = "Anxiety/Fear Level Profile", fileName = "NewFearLevelProfile")]
public class FearLevelProfile : ScriptableObject
{
    [Header("Timing Settings (passive profiles only)")]
    public float minInterval;
    public float maxInterval;
    public float minDuration;
    public float maxDuration;

    [Header("Anxiety effects for this profile")]
    public List<BaseFearEffect> effects = new();
}