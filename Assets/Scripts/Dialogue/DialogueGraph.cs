using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraph : ScriptableObject {
    public DialogueNode startNode;         // Startowy (domyœlny) wêze³ dialogu
    public List<DialogueNode> nodes = new List<DialogueNode>();  // Lista wszystkich wêz³ów w dialogu
}