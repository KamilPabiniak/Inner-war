using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/End Node")]
public class DialogueEndNode : DialogueNode {
    private void OnEnable() {
        nodeType = NodeType.End;
        dialogueText = "End Dialogue";
    }

    public override bool CanHaveOutgoingConnections() {
        return false;
    }
}