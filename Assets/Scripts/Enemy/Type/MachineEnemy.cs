using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MachineEnemy : EnemyBase
{
    [Header("Head Rotation Settings")]
    public GameObject head;
    public Quaternion originalHeadRot;
    public float headRotationSpeed;
    public int rotationStopPoints;
    public float rotationStopDuration = 0.5f;


    private void Start()
    {
        ChangeState(new PatrolState());
        originalHeadRot = head.transform.localRotation;
    }
}

