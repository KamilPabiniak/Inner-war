using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Start Node")]
public class DialogueStartNode : DialogueNode {
    private void OnEnable() {
        nodeType = NodeType.Start;
        dialogueText = "Start Dialogue";
    }

    public override bool CanHaveOutgoingConnections() {
        // Startowy wêze³ jest specjalny – nie powinien byæ ³¹czony z innych (tylko wychodz¹ce po³¹czenia)
        return true;
    }
}