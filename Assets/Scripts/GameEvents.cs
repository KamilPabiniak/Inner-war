using System;

public static class GameEvents
{
    //CheatSheet
    public static Action onTogglePanel;
    
    //Kill
    public static Action onPlayerKilled;
    
    //Save
    public static Action onSaveCheckpoint;
    
    //Death
    public static Action onPlayerDied;
    public static Action onPlayerRespawned;
    
    // BlackScreen - FadeTime - Duration
    public static Action<float, float, float> onBlackScreen;
}