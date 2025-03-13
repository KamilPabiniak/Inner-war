using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DialogueGraphEditorWindow : EditorWindow {
    private DialogueGraph dialogueGraph;

    private Vector2 offset;
    private Vector2 drag;

    private const float NODE_WIDTH = 200;
    private const float NODE_HEIGHT = 100;

    private DialogueNode selectedNode = null;
    private DialogueNode nodeToConnectFrom = null;

    // Do obs³ugi przeci¹gania pojedynczego wêz³a
    private bool isDraggingNode = false;
    private Vector2 dragOffset;

    [MenuItem("Window/Dialogue Graph Editor")]
    public static void OpenWindow() {
        GetWindow<DialogueGraphEditorWindow>("Dialogue Graph Editor");
    }

    private void OnEnable() {
        // £adujemy lub tworzymy asset DialogueGraph w sta³ej lokalizacji
        string path = "Assets/DialogueGraph.asset";
        dialogueGraph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
        if (dialogueGraph == null) {
            dialogueGraph = ScriptableObject.CreateInstance<DialogueGraph>();

            // Tworzymy domyœlny startowy wêze³ dialogu
            DialogueStartNode startNode = ScriptableObject.CreateInstance<DialogueStartNode>();
            startNode.nodeID = System.Guid.NewGuid().ToString();
            startNode.position = new Vector2(50, 50);
            dialogueGraph.startNode = startNode;
            dialogueGraph.nodes.Add(startNode);

            AssetDatabase.CreateAsset(startNode, "Assets/StartNode.asset");
            AssetDatabase.CreateAsset(dialogueGraph, path);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI() {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawNodes();

        ProcessEvents(Event.current);

        if (GUI.changed)
            Repaint();
    }

    #region Rysowanie siatki i elementów

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0) + newOffset,
                             new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }
        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(new Vector3(0, gridSpacing * j, 0) + newOffset,
                             new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes() {
        if (dialogueGraph == null)
            return;

        foreach (DialogueNode node in dialogueGraph.nodes) {
            Rect nodeRect = new Rect(node.position.x, node.position.y, NODE_WIDTH, NODE_HEIGHT);
            // Jeœli wêze³ jest zaznaczony – umo¿liwiamy edycjê tekstu bezpoœrednio
            if (selectedNode == node) {
                EditorGUI.BeginChangeCheck();
                string newText = EditorGUI.TextField(new Rect(nodeRect.x + 10, nodeRect.y + 10, nodeRect.width - 20, 20), node.dialogueText);
                if (EditorGUI.EndChangeCheck()) {
                    node.dialogueText = newText;
                    EditorUtility.SetDirty(node);
                }
            }
            GUI.Box(nodeRect, node.dialogueText);
        }
    }

    private void DrawConnections() {
        if (dialogueGraph == null)
            return;

        foreach (DialogueNode node in dialogueGraph.nodes) {
            Rect fromRect = new Rect(node.position.x, node.position.y, NODE_WIDTH, NODE_HEIGHT);
            foreach (string targetID in node.connections) {
                DialogueNode targetNode = dialogueGraph.nodes.Find(n => n.nodeID == targetID);
                if (targetNode != null) {
                    Rect toRect = new Rect(targetNode.position.x, targetNode.position.y, NODE_WIDTH, NODE_HEIGHT);
                    Vector3 startPos = new Vector3(fromRect.x + fromRect.width, fromRect.y + fromRect.height / 2, 0);
                    Vector3 endPos = new Vector3(toRect.x, toRect.y + toRect.height / 2, 0);
                    Handles.DrawLine(startPos, endPos);
                }
            }
        }
    }

    #endregion

    #region Obs³uga zdarzeñ (drag, context menu, itd.)

    private void ProcessEvents(Event e) {
        drag = Vector2.zero;
        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0) { // lewy przycisk – zaznaczanie/rozpoczêcie przeci¹gania
                    DialogueNode nodeClicked = GetNodeAtPoint(e.mousePosition);
                    if (nodeClicked != null) {
                        selectedNode = nodeClicked;
                        isDraggingNode = true;
                        dragOffset = e.mousePosition - selectedNode.position;
                    }
                    else {
                        selectedNode = null;
                    }
                }
                break;
            case EventType.MouseDrag:
                if (isDraggingNode && selectedNode != null) {
                    selectedNode.position = e.mousePosition - dragOffset;
                    GUI.changed = true;
                }
                else if (e.button == 0) {
                    OnDrag(e.delta);
                }
                break;
            case EventType.MouseUp:
                isDraggingNode = false;
                break;
            case EventType.ContextClick:
                ShowContextMenu(e.mousePosition);
                break;
        }
    }

    private void OnDrag(Vector2 delta) {
        drag = delta;
        offset += delta;
        // Przeci¹gamy wszystkie wêz³y razem (ca³a siatka)
        if (dialogueGraph != null) {
            foreach (DialogueNode node in dialogueGraph.nodes) {
                node.position += delta;
            }
        }
        GUI.changed = true;
    }

    private DialogueNode GetNodeAtPoint(Vector2 point) {
        if (dialogueGraph != null) {
            foreach (DialogueNode node in dialogueGraph.nodes) {
                Rect nodeRect = new Rect(node.position.x, node.position.y, NODE_WIDTH, NODE_HEIGHT);
                if (nodeRect.Contains(point))
                    return node;
            }
        }
        return null;
    }

    private void ShowContextMenu(Vector2 mousePosition) {
        GenericMenu menu = new GenericMenu();

        // Dodawanie nowych wêz³ów – dostêpne typy
        menu.AddItem(new GUIContent("Add Dialogue Node"), false, () => OnClickAddNode(NodeType.Dialogue, mousePosition));
        menu.AddItem(new GUIContent("Add Choice Node"), false, () => OnClickAddNode(NodeType.Choice, mousePosition));
        menu.AddItem(new GUIContent("Add End Node"), false, () => OnClickAddNode(NodeType.End, mousePosition));
        menu.AddItem(new GUIContent("Add Option Node"), false, () => OnClickAddNode(NodeType.Option, mousePosition));

        if (selectedNode != null) {
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete Node"), false, () => OnClickDeleteNode(selectedNode));
            menu.AddItem(new GUIContent("Start Connection From Node"), false, () => { nodeToConnectFrom = selectedNode; });
        }

        // Jeœli mamy wybrany wêze³ z którego chcemy po³¹czyæ oraz inny zaznaczony – opcja po³¹czenia
        if (nodeToConnectFrom != null && selectedNode != null && selectedNode != nodeToConnectFrom) {
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Connect Node"), false, () => OnClickConnectNode(nodeToConnectFrom, selectedNode));
        }
        menu.ShowAsContext();
    }

    #endregion

    #region Akcje z menu kontekstowego

    private void OnClickAddNode(NodeType type, Vector2 mousePosition) {
        DialogueNode newNode = null;
        string assetPath = "";
        switch (type) {
            case NodeType.Dialogue:
                newNode = ScriptableObject.CreateInstance<DialogueTextNode>();
                assetPath = "Assets/DialogueTextNode_" + System.Guid.NewGuid().ToString() + ".asset";
                break;
            case NodeType.Choice:
                newNode = ScriptableObject.CreateInstance<DialogueChoiceNode>();
                assetPath = "Assets/DialogueChoiceNode_" + System.Guid.NewGuid().ToString() + ".asset";
                break;
            case NodeType.End:
                newNode = ScriptableObject.CreateInstance<DialogueEndNode>();
                assetPath = "Assets/DialogueEndNode_" + System.Guid.NewGuid().ToString() + ".asset";
                break;
            case NodeType.Option:
                newNode = ScriptableObject.CreateInstance<DialogueOptionNode>();
                assetPath = "Assets/DialogueOptionNode_" + System.Guid.NewGuid().ToString() + ".asset";
                break;
        }
        if (newNode != null) {
            newNode.nodeID = System.Guid.NewGuid().ToString();
            newNode.dialogueText = type.ToString() + " Node";
            newNode.position = mousePosition;

            // Nie pozwalamy tworzyæ dodatkowego wêz³a startowego (startowy jest tworzony raz)
            if (type == NodeType.Start) {
                Debug.LogError("Cannot create additional start node.");
                return;
            }
            dialogueGraph.nodes.Add(newNode);
            AssetDatabase.CreateAsset(newNode, assetPath);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnClickDeleteNode(DialogueNode node) {
        // Usuwamy wszelkie po³¹czenia do tego wêz³a
        foreach (DialogueNode n in dialogueGraph.nodes) {
            n.connections.Remove(node.nodeID);
        }
        dialogueGraph.nodes.Remove(node);
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(node));
        AssetDatabase.SaveAssets();
    }

    private void OnClickConnectNode(DialogueNode from, DialogueNode to) {
        if (from != null && to != null && from.CanHaveOutgoingConnections()) {
            if (!from.connections.Contains(to.nodeID)) {
                from.connections.Add(to.nodeID);
                EditorUtility.SetDirty(from);
            }
        }
        nodeToConnectFrom = null;
    }

    #endregion
}
