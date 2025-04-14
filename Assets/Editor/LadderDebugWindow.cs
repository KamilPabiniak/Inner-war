using System;
using UnityEngine;
using UnityEditor;

public class LadderDebugWindow : EditorWindow
{
    // Reference to the ladder object to be debugged.
    public Ladder selectedLadder;
    
    private CharacterController _playerController;

    // Debug options.
    public bool showPlayerCollider;
    public Color colliderColor = Color.cyan;

    [MenuItem("Window/Ladder Debug Window")]
    public static void OpenWindow()
    {
        GetWindow<LadderDebugWindow>("Ladder Debugger");
    }

    // Static method to open the window and set the selected ladder + enable collider debugging.
    public static void ShowLadder(Ladder ladder)
    {
        LadderDebugWindow window = GetWindow<LadderDebugWindow>("Ladder Debugger");
        window.selectedLadder = ladder;
        window.showPlayerCollider = true;
        window.Repaint();
    }

    [Obsolete("Obsolete")]
    private void OnEnable()
    {
        // Subscribe to the SceneView drawing event.
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [Obsolete("Obsolete")]
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Ladder Debug Options", EditorStyles.boldLabel);

        // Select the ladder object.
        selectedLadder = (Ladder)EditorGUILayout.ObjectField("Ladder Object", selectedLadder, typeof(Ladder), true);

        // Debug settings.
        showPlayerCollider = EditorGUILayout.Toggle("Show Player Collider", showPlayerCollider);
        colliderColor = EditorGUILayout.ColorField("Collider Color", colliderColor);

        if (selectedLadder == null)
        {
            EditorGUILayout.HelpBox("Select a ladder object from the hierarchy to debug.", MessageType.Info);
        }

        // Force a SceneView repaint when settings change.
        SceneView.RepaintAll();
    }

    [Obsolete("Obsolete")]
    private void OnSceneGUI(SceneView sceneView)
    {
        if (selectedLadder == null)
            return;

        // Get the ladder transform.
        Transform ladderTransform = selectedLadder.transform;
        Quaternion ladderRotation = ladderTransform.rotation;
        var position1 = ladderTransform.position;
        Vector3 basePos = position1 + ladderRotation * new Vector3(0, 0, selectedLadder.ladderOffset);

        // Calculate the ladder points.
        Vector3 lowerPoint = new Vector3(basePos.x, position1.y + selectedLadder.lowerClimbPointOffset, basePos.z);
        Vector3 upperPoint = new Vector3(basePos.x, position1.y + selectedLadder.upperClimbPointOffset, basePos.z);

        // Draw the ladder start points.
        Handles.color = Color.green;
        Handles.SphereHandleCap(0, lowerPoint, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.color = Color.blue;
        Handles.SphereHandleCap(0, upperPoint, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.color = Color.yellow;
        Handles.DrawLine(lowerPoint, upperPoint);

        // Calculate and draw the ladder exit points.
        Vector3 lowerExitPoint = lowerPoint + ladderRotation * selectedLadder.bottomExitLocalOffset;
        Vector3 upperExitPoint = upperPoint + ladderRotation * selectedLadder.topExitLocalOffset;

        Handles.color = Color.magenta;
        Handles.DrawLine(lowerPoint, lowerExitPoint);
        Handles.SphereHandleCap(0, lowerExitPoint, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.color = Color.red;
        Handles.DrawLine(upperPoint, upperExitPoint);
        Handles.SphereHandleCap(0, upperExitPoint, Quaternion.identity, 0.2f, EventType.Repaint);

        // If the option to show the player's collider is active.
        if (showPlayerCollider)
        {
            if (_playerController == null)
            {
                // Try to find the first instance of CharacterController in the scene.
                _playerController = FindObjectOfType<CharacterController>();
            }

            if (_playerController != null)
            {
                Vector3 colliderSize = new Vector3(_playerController.radius * 2f, _playerController.height, _playerController.radius * 2f);
                Handles.color = colliderColor;
                Handles.DrawWireCube(lowerExitPoint, colliderSize);
                Handles.DrawWireCube(upperExitPoint, colliderSize);
            }
        }
    }
}
