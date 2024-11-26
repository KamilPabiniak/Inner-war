using UnityEngine;

public abstract class PlayerModule : MonoBehaviour
{
    protected Player Player { get; private set; }

    public void Initialize(Player player)
    {
        Player = player;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}