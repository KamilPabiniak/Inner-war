using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BombInteract : MonoBehaviour, IInteractable
{
    public void Interact(Player player)
    {
        SceneManager.LoadScene("DemoEnd", LoadSceneMode.Single);
    }
}
