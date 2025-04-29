using UnityEditor;
using UnityEngine;

public class ColliderAdjusterWindow : EditorWindow
{
    private GameObject _parentObject;
    private float _margin;
    private bool _ignoreChildren;
    private enum BoundsMode { MeshFilterBounds, RendererBounds }
    private BoundsMode _boundsMode = BoundsMode.MeshFilterBounds;


    [MenuItem("Tools/Collider Adjuster")]
    public static void ShowWindow()
    {
        GetWindow<ColliderAdjusterWindow>("Collider Adjuster");
    }

    private void OnGUI()
    {
        GUILayout.Label("Adjust Collider to Fit Mesh Bounds", EditorStyles.boldLabel);
        _parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", _parentObject, typeof(GameObject), true);

        EditorGUILayout.Space();
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        _margin = EditorGUILayout.FloatField("Margin", _margin);
        _ignoreChildren = EditorGUILayout.Toggle("Ignore Children", _ignoreChildren);
        _boundsMode = (BoundsMode)EditorGUILayout.EnumPopup("Bounds Source", _boundsMode);


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

        if (_boundsMode == BoundsMode.MeshFilterBounds)
        {
            if (_ignoreChildren)
            {
                MeshFilter mf = _parentObject.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    combinedBounds = mf.sharedMesh.bounds;
                    boundsInitialized = true;
                }
            }
            else
            {
                foreach (MeshFilter mf in _parentObject.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.sharedMesh == null) continue;
                    Bounds worldBounds = TransformBounds(mf.transform.localToWorldMatrix, mf.sharedMesh.bounds);
                    if (!boundsInitialized)
                    {
                        combinedBounds = worldBounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(worldBounds);
                    }
                }

                if (boundsInitialized)
                    combinedBounds = TransformBounds(Matrix4x4.Inverse(_parentObject.transform.localToWorldMatrix), combinedBounds);
            }
        }
        else if (_boundsMode == BoundsMode.RendererBounds)
        {
            if (!_ignoreChildren)
            {
                foreach (Renderer rend in _parentObject.GetComponentsInChildren<Renderer>())
                {
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
            else
            {
                Renderer rend = _parentObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    combinedBounds = rend.bounds;
                    boundsInitialized = true;
                }
            }

            if (boundsInitialized)
                combinedBounds = TransformBounds(Matrix4x4.Inverse(_parentObject.transform.localToWorldMatrix), combinedBounds);
        }

        if (!boundsInitialized)
        {
            Debug.LogWarning("No valid data found to compute bounds.");
            return;
        }

        combinedBounds.Expand(_margin);
        ApplyCollider(combinedBounds);
    }


    private void ApplyCollider(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        Vector3 lossyScale = _parentObject.transform.lossyScale;

        BoxCollider boxCol = _parentObject.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Undo.RecordObject(boxCol, "Adjust BoxCollider");
            boxCol.center = center;
            boxCol.size = new Vector3(
                size.x / lossyScale.x,
                size.y / lossyScale.y,
                size.z / lossyScale.z
            );
            Debug.Log("BoxCollider adjusted.");
            return;
        }

        SphereCollider sphereCol = _parentObject.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            float worldRadius = Mathf.Max(size.x, Mathf.Max(size.y, size.z)) * 0.5f;
            float maxScale = Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
            float localRadius = worldRadius / maxScale;

            Undo.RecordObject(sphereCol, "Adjust SphereCollider");
            sphereCol.center = center;
            sphereCol.radius = localRadius;
            Debug.Log("SphereCollider adjusted.");
            return;
        }

        CapsuleCollider capsuleCol = _parentObject.GetComponent<CapsuleCollider>();
        if (capsuleCol != null)
        {
            int direction = capsuleCol.direction;
            float radius = 0f, height = 0f;
            switch (direction)
            {
                case 0: radius = Mathf.Min(size.y, size.z) * 0.5f; height = size.x; break;
                case 1: radius = Mathf.Min(size.x, size.z) * 0.5f; height = size.y; break;
                case 2: radius = Mathf.Min(size.x, size.y) * 0.5f; height = size.z; break;
            }

            Undo.RecordObject(capsuleCol, "Adjust CapsuleCollider");
            capsuleCol.center = center;
            capsuleCol.radius = radius / Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
            capsuleCol.height = height / lossyScale[direction];
            Debug.Log("CapsuleCollider adjusted.");
            return;
        }

        Debug.LogWarning("No supported collider found on object.");
    }

    private Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
    {
        var center = matrix.MultiplyPoint(bounds.center);

        // Calculate 8 corners of the bounds and transform them
        Vector3 extents = bounds.extents;
        Vector3[] points = new Vector3[8];
        points[0] = center + new Vector3(extents.x, extents.y, extents.z);
        points[1] = center + new Vector3(-extents.x, extents.y, extents.z);
        points[2] = center + new Vector3(extents.x, -extents.y, extents.z);
        points[3] = center + new Vector3(extents.x, extents.y, -extents.z);
        points[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
        points[5] = center + new Vector3(extents.x, -extents.y, -extents.z);
        points[6] = center + new Vector3(-extents.x, extents.y, -extents.z);
        points[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);

        Bounds newBounds = new Bounds(matrix.MultiplyPoint(bounds.center), Vector3.zero);
        foreach (Vector3 v in points)
            newBounds.Encapsulate(v);

        return newBounds;
    }
}
