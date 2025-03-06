using System.Collections.Generic;
using Anxiety.Effects;
using UnityEngine;

[CreateAssetMenu(menuName = "Anxiety/Fear Level Profile", fileName = "NewFearLevelProfile")]
public class FearLevelProfile : ScriptableObject
{
    [Header("Efekty dla danego poziomu lêku")]
    [Tooltip("Lista efektów (konkretnych assetów dziedzicz¹cych po BaseFearEffect), które maj¹ byæ wywo³ywane przy aktywnym poziomie lêku.")]
    public List<BaseFearEffect> effects = new List<BaseFearEffect>();
}