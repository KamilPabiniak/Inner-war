using UnityEngine;
public class ShelfIdentifier : MonoBehaviour
{
    [HideInInspector] public string uniqueID;
    public string baseName;

    private void Reset()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = System.Guid.NewGuid().ToString();
    }
}
