using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Text Node")]
public class DialogueTextNode : DialogueNode {
    private void OnEnable() {
        nodeType = NodeType.Dialogue;
        dialogueText = "Dialogue Text";
    }
}