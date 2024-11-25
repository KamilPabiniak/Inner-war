using UnityEngine;

public abstract class PlayerModule : MonoBehaviour
{
    protected Player.Player Player { get; private set; }

    public void Initialize(Player.Player player)
    {
        Player = player;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}