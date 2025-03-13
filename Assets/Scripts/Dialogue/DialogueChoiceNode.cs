using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Choice Node")]
public class DialogueChoiceNode : DialogueNode {
    private void OnEnable() {
        nodeType = NodeType.Choice;
        dialogueText = "Choice Text";
    }
}