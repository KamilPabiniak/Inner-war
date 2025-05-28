using UnityEngine;

public class FootstepAudioOnPlant : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAudio enemyAudio;
    [SerializeField] private float rayYOffset = 1f;
    [SerializeField] private float rayDistance = 0.1f;
    [SerializeField] private float plantedYOffset = 0.1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField]private AnimationRiggingFootPlanter planter;
    private bool wasPlantedLastFrame;
    

    private void LateUpdate()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * rayYOffset;
        bool planted = false;
    
        if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayDistance, groundMask))
        {
            float footY      = planter.FootPosition.y;
            float targetY    = hit.point.y + plantedYOffset;
            if (footY < targetY)
            {
                planted = true;
            }
        }
    
        if (planted && !wasPlantedLastFrame)
        {
            enemyAudio.PlayFootStepSound();
        }
        else if (!planted && wasPlantedLastFrame)
        {
            enemyAudio.ResetFootStepFlag();
        }

        wasPlantedLastFrame = planted;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 1) Pozycja pocz¹tkowa promienia
        Vector3 rayOrigin = transform.position + Vector3.up * rayYOffset;
        Vector3 rayEnd    = rayOrigin + Vector3.down * rayDistance;

        // Rysuj liniê raycasta
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin, rayEnd);

        // 2) Jeœli jesteœmy blisko ziemi, poka¿ punkt „plantowania”
        if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayDistance, groundMask))
        {
            Vector3 plantPos = hit.point + Vector3.up * plantedYOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(plantPos, 0.05f);
        }

        // 3) Pozycja stopy z riggowania
        if (planter != null)
        {
            Vector3 footPos = planter.FootPosition;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(footPos, 0.05f);
        }
    }
#endif

}