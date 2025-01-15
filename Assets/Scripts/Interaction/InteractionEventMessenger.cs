using System;
using UnityEngine;

public static class InteractionEventMessenger
{
    public static event Action<string> OnShowInteractionText;
    
    public static event Action OnHideInteractionText;
    
    public static void ShowInteractionText(string interactionText)
    {
        OnShowInteractionText?.Invoke(interactionText);
    }
    
    public static void HideInteractionText()
    {
        OnHideInteractionText?.Invoke();
    }
}
