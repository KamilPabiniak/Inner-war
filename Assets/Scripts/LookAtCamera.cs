using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.rotation = Quaternion.LookRotation(-transform.forward);
        transform.position = new Vector3(pivot.position.x, transform.position.y, pivot.position.z);
    }
}
