using UnityEngine;
using System.Collections.Generic;

public enum NodeType {
    Start,
    Dialogue,
    Choice,
    End,
    Option
}

public abstract class DialogueNode : ScriptableObject {
    public string nodeID;                   // Unikalny identyfikator wêz³a
    public NodeType nodeType;               // Typ wêz³a
    [TextArea]
    public string dialogueText;             // Tekst wypowiedzi lub opcji
    public List<string> connections = new List<string>();  // Lista ID wêz³ów, do których prowadzi ten wêze³
    public Vector2 position;                // Pozycja w edytorze (zapisywana w assetach)

    public virtual bool CanHaveOutgoingConnections() {
        // Domyœlnie wêze³ mo¿e mieæ po³¹czenia, chyba ¿e jest typu End lub Option
        return nodeType != NodeType.End && nodeType != NodeType.Option;
    }
}