using System.Collections.Generic;
using Anxiety.Effects;
using UnityEngine;

[CreateAssetMenu(menuName = "Anxiety/Fear Level Profile", fileName = "NewFearLevelProfile")]
public class FearLevelProfile : ScriptableObject
{
    [Header("Anxiety effect for this profile")]
    [HideInInspector] public List<BaseFearEffect> effects = new();
}