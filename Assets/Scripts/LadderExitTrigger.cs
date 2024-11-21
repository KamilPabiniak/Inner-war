using UnityEngine;

public class LadderExitTrigger : MonoBehaviour
{
    public bool isTop;
    public Ladder ladder;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (ladder.IsClimbing()) 
            {
                ladder.ExitLadder(isTop);
            }
        }
    }
}
