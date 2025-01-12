using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class MachineEnemy : EnemyBase
{
    [AdvancedHeader("Machine Specification", fontSize: 16, bottomSpace: 15f, alignment: TextAnchor.MiddleLeft)]
    [Header("References")]
    public GameObject head;

    public GameObject sightTarget;
    public Vector3 OriginalHeadPos { get; private set; }

    [AdvancedHeader("Patrol Head Specification", fontSize: 11, bottomSpace: 15f, alignment: TextAnchor.MiddleLeft)]
    public float headRotationSpeed;
    public int stopPoints;
    public float stopDuration = 0.5f;

    private void Start()
    {
        OriginalHeadPos = sightTarget.transform.localPosition;
        ChangeState(new PatrolState());
    }

    [ContextMenu("Patrol")]
    public void ForcePatrol()
    {
        ChangeState(new PatrolState());
    }
}

