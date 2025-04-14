using UnityEditor;
using UnityEngine;

public class ColliderAdjusterWindow : EditorWindow
{
    private GameObject _parentObject;
    private float _margin;

    [MenuItem("Tools/Collider Adjuster")]
    public static void ShowWindow()
    {
        GetWindow<ColliderAdjusterWindow>("Collider Adjuster");
    }

    private void OnGUI()
    {
        GUILayout.Label("Adjust Parent BoxCollider to Enclose Children", EditorStyles.boldLabel);
        _parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", _parentObject, typeof(GameObject), true);

        EditorGUILayout.Space();
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        _margin = EditorGUILayout.FloatField("Margin", _margin);

        if (_parentObject == null)
        {
            EditorGUILayout.HelpBox("Please select a parent object.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Adjust Collider"))
        {
            AdjustCollider();
        }
    }

    private void AdjustCollider()
    {
        if (_parentObject == null)
        {
            Debug.LogWarning("No parent object selected.");
            return;
        }

        BoxCollider boxCol = _parentObject.GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            Debug.LogWarning("The parent object does not have a BoxCollider component.");
            return;
        }

        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;
        
        Renderer[] childRenderers = _parentObject.GetComponentsInChildren<Renderer>();
        if (childRenderers is { Length: > 0 })
        {
            foreach (Renderer rend in childRenderers)
            {
                if (rend.gameObject == _parentObject)
                    continue;

                if (!boundsInitialized)
                {
                    combinedBounds = rend.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(rend.bounds);
                }
            }
        }
        
        if (!boundsInitialized)
        {
            Transform[] children = _parentObject.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child == _parentObject.transform)
                    continue;

                if (!boundsInitialized)
                {
                    combinedBounds = new Bounds(child.position, Vector3.zero);
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(child.position);
                }
            }
        }

        if (!boundsInitialized)
        {
            Debug.LogWarning("No child objects found to compute bounds.");
            return;
        }
        
        combinedBounds.Expand(_margin);
        
        Vector3 localCenter = _parentObject.transform.InverseTransformPoint(combinedBounds.center);

        var lossyScale = _parentObject.transform.lossyScale;
        Vector3 localSize = new Vector3(
            combinedBounds.size.x / lossyScale.x,
            combinedBounds.size.y / lossyScale.y,
            combinedBounds.size.z / lossyScale.z
        );
        
        Undo.RecordObject(boxCol, "Adjust Collider");

        boxCol.center = localCenter;
        boxCol.size = localSize;

        Debug.Log("Collider adjusted to fit all child objects with margin " + _margin + ".");
    }
}
