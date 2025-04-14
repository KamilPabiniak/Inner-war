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
        GUILayout.Label("Adjust Parent Collider to Enclose Children", EditorStyles.boldLabel);
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
        Vector3 lossyScale = _parentObject.transform.lossyScale;
        
        // 1. BoxCollider
        BoxCollider boxCol = _parentObject.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Vector3 localSize = new Vector3(
                combinedBounds.size.x / lossyScale.x,
                combinedBounds.size.y / lossyScale.y,
                combinedBounds.size.z / lossyScale.z
            );
            
            Undo.RecordObject(boxCol, "Adjust BoxCollider");
            boxCol.center = localCenter;
            boxCol.size = localSize;
            Debug.Log("BoxCollider adjusted with margin " + _margin + ".");
            return;
        }

        // 2. SphereCollider
        SphereCollider sphereCol = _parentObject.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            Vector3 localPos = localCenter;
            float worldRadius = Mathf.Max(combinedBounds.size.x, Mathf.Max(combinedBounds.size.y, combinedBounds.size.z)) * 0.5f;
            float maxScale = Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
            float localRadius = worldRadius / maxScale;
            
            Undo.RecordObject(sphereCol, "Adjust SphereCollider");
            sphereCol.center = localPos;
            sphereCol.radius = localRadius;
            Debug.Log("SphereCollider adjusted with margin " + _margin + ".");
            return;
        }

        // 3. CapsuleCollider
        CapsuleCollider capsuleCol = _parentObject.GetComponent<CapsuleCollider>();
        if (capsuleCol != null)
        {
            Vector3 localPos = localCenter;
            int direction = capsuleCol.direction;
            Vector3 localSize = new Vector3(
                combinedBounds.size.x / lossyScale.x,
                combinedBounds.size.y / lossyScale.y,
                combinedBounds.size.z / lossyScale.z
            );

            float radius = 0f, height = 0f;
            switch (direction)
            {
                case 0: 
                    radius = Mathf.Min(localSize.y, localSize.z) * 0.5f;
                    height = localSize.x;
                    break;
                case 1: 
                    radius = Mathf.Min(localSize.x, localSize.z) * 0.5f;
                    height = localSize.y;
                    break;
                case 2:
                    radius = Mathf.Min(localSize.x, localSize.y) * 0.5f;
                    height = localSize.z;
                    break;
            }

            Undo.RecordObject(capsuleCol, "Adjust CapsuleCollider");
            capsuleCol.center = localPos;
            capsuleCol.radius = radius;
            capsuleCol.height = height;
            Debug.Log("CapsuleCollider adjusted with margin " + _margin + ".");
            return;
        }

        Debug.LogWarning("No supported collider component found on the parent object. Please attach a BoxCollider, SphereCollider, or CapsuleCollider.");
    }
}
