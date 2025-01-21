using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionHighlight : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string[] interactableTags;
    public float interactionRadius = 5f;

    private Dictionary<GameObject, Coroutine> activeHighlights = new Dictionary<GameObject, Coroutine>();
    private static readonly int RimRange = Shader.PropertyToID("_Rim_Range");
    private static readonly int RimBlend = Shader.PropertyToID("_Rim_Blend");
    private static readonly int RimColor = Shader.PropertyToID("_Rim_Color");

    private void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius);

        HashSet<GameObject> detectedObjects = new HashSet<GameObject>();
        
        List<GameObject> objectsToAdd = new List<GameObject>();
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var collider in hitColliders)
        {
            GameObject obj = collider.gameObject;
            bool isInteractable = HasValidTag(obj);

            if (isInteractable)
            {
                Debug.Log("is Interactable");

                if (obj.TryGetComponent(out Renderer renderer))
                {
                    detectedObjects.Add(obj);
                    if (!activeHighlights.ContainsKey(obj) && renderer.material.HasProperty(RimRange) && renderer.material.HasProperty(RimBlend))
                    {
                        objectsToAdd.Add(obj);
                    }
                }
            }
        }
        
        foreach (var obj in objectsToAdd)
        {
            if (obj.TryGetComponent(out Renderer renderer))
            {
                Coroutine highlightCoroutine = StartCoroutine(AnimateRimEffects(renderer.material, 1.5f, 0.5f));
                activeHighlights[obj] = highlightCoroutine;
            }
        }
        
        foreach (var obj in activeHighlights.Keys)
        {
            if (!detectedObjects.Contains(obj))
            {
                objectsToRemove.Add(obj); 
            }
        }
        
        foreach (var obj in objectsToRemove)
        {
            StopCoroutine(activeHighlights[obj]);
            activeHighlights.Remove(obj);

            if (obj.TryGetComponent(out Renderer renderer))
            {
                ResetRimEffects(renderer.material);
            }
        }
    }

    private bool HasValidTag(GameObject obj)
    {
        foreach (var tag in interactableTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator AnimateRimEffects(Material material, float targetRimRange, float targetRimBlend)
    {
        float rimRange = material.GetFloat(RimRange);
        float rimBlend = material.GetFloat(RimBlend);
        bool increasing = true;

        while (true)
        {
            rimRange = increasing
                ? Mathf.MoveTowards(rimRange, targetRimRange, Time.deltaTime)
                : Mathf.MoveTowards(rimRange, 0, Time.deltaTime);

            rimBlend = increasing
                ? Mathf.MoveTowards(rimBlend, targetRimBlend, Time.deltaTime)
                : Mathf.MoveTowards(rimBlend, 0, Time.deltaTime);

            material.SetFloat(RimRange, rimRange);
            material.SetFloat(RimBlend, rimBlend);

            if (increasing && Mathf.Approximately(rimRange, targetRimRange) && Mathf.Approximately(rimBlend, targetRimBlend))
                increasing = false;
            else if (!increasing && Mathf.Approximately(rimRange, 0) && Mathf.Approximately(rimBlend, 0))
                increasing = true;

            yield return null;
        }
    }

    private void ResetRimEffects(Material material)
    {
        if (material.HasProperty(RimRange))
        {
            material.SetFloat(RimRange, 0);
        }

        if (material.HasProperty(RimBlend))
        {
            material.SetFloat(RimBlend, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
