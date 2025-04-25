using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolArea))]
public class PatrolAreaEditor : Editor
{
    private const string DontShowAgainKey = "PatrolAreaInfoPopup_DoNotShow";

    private void OnEnable()
    {
        if (!EditorPrefs.GetBool(DontShowAgainKey, false))
        {
            bool showAgain = EditorUtility.DisplayDialog(
                "PatrolArea Warning",
                "IMPORTANT: 'PatrolArea' is an *optional* component used to define precise patrol zones for enemies.\n\nThis script is NOT required for AI to function.\n\nIf you assign it to an enemy, make sure to configure patrol zones correctly!\n\nYou must manually assign this component to the 'EnemyBrain' script's patrolArea field.",
                "OK", "Don't show this again"
            );

            if (!showAgain)
            {
                EditorPrefs.SetBool(DontShowAgainKey, true);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}