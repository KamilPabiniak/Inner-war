using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ladder))]
public class LadderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!GUILayout.Button("Show Player Colliders")) return;
        Ladder ladder = (Ladder)target;
        LadderDebugWindow.ShowLadder(ladder);
    }
}