using UnityEngine;
using UnityEngine.Serialization;

public class MachineEnemy : EnemyBase
{
    public GameObject head;
    public Quaternion originalHeadRot;
    public float headRotationSpeed;

    private void Start()
    {
        ChangeState(new PatrolState());
        originalHeadRot = head.transform.localRotation;
    }
}