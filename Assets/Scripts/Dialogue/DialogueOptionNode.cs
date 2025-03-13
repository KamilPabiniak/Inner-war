using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Option Node")]
public class DialogueOptionNode : DialogueNode {
    private void OnEnable() {
        nodeType = NodeType.Option;
        dialogueText = "Option Text";
    }

    public override bool CanHaveOutgoingConnections() {
        return false;
    }
}