using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour, IInteractable
{
    public void Interact(Player player)
    {
        SceneManager.LoadScene("DemoEnd", LoadSceneMode.Single);
    }
}
