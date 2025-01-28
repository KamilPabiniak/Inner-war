using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.rotation = Quaternion.LookRotation(-transform.forward);
    }
}
