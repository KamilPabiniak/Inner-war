using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QuestSystem;  // <-- upewnij siê, ¿e namespace QuestSystem jest dostêpny

public class InteractionHighlight : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string[] interactableTags;
    public float interactionRadius = 5f;

    [Header("Debug")] 
    [SerializeField] private bool showHighlightRange;

    private Dictionary<GameObject, Coroutine> activeHighlights = new();
    private static readonly int RimRange = Shader.PropertyToID("_Rim_Range");
    private static readonly int RimBlend = Shader.PropertyToID("_Rim_Blend");
    private static readonly int RimColor = Shader.PropertyToID("_Rim_Color");

    private void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius);

        // Zbiór obiektów, które w tej klatce powinny byæ podœwietlone
        HashSet<GameObject> detectedObjects = new HashSet<GameObject>();
        
        List<GameObject> objectsToAdd = new List<GameObject>();
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var collider in hitColliders)
        {
            GameObject obj = collider.gameObject;
            // SprawdŸmy najpierw, czy obiekt w ogóle ma poprawny tag
            if (!HasValidTag(obj))
                continue;

            // Je¿eli obiekt ma QuestInteractable, to sprawdŸmy, czy jest to aktualny quest
            QuestInteractable qi = obj.GetComponent<QuestInteractable>();
            if (qi != null)
            {
                Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
                // Jeœli nie ma w ogóle aktywnego questa, albo ID siê nie zgadza, pomijamy go
                if (currentQuest == null || qi.associatedQuestID != currentQuest.questID)
                    continue;
            }

            // Po tych filtrach – obiekt kwalifikuje siê do podœwietlania
            if (obj.TryGetComponent(out Renderer renderer))
            {
                detectedObjects.Add(obj);
                // Jeœli jeszcze nie ma coroutine, a materia³ ma odpowiednie w³aœciwoœci -> dodajemy do animacji
                if (!activeHighlights.ContainsKey(obj) 
                    && renderer.material.HasProperty(RimRange) 
                    && renderer.material.HasProperty(RimBlend))
                {
                    objectsToAdd.Add(obj);
                }
            }
        }
        
        // Uruchomienie podœwietlenia dla nowych obiektów
        foreach (var obj in objectsToAdd)
        {
            if (obj == null) continue;
            if (obj.TryGetComponent(out Renderer renderer))
            {
                Coroutine highlightCoroutine = StartCoroutine(AnimateRimEffects(renderer.material, 1.5f, 0.5f));
                activeHighlights[obj] = highlightCoroutine;
            }
        }
        
        // SprawdŸmy, które obiekty opuœci³y zakres albo zosta³y odfiltrowane – musimy usun¹æ z aktywnego s³ownika
        foreach (var kvp in activeHighlights)
        {
            GameObject obj = kvp.Key;
            if (!detectedObjects.Contains(obj))
            {
                objectsToRemove.Add(obj);
            }
        }

        foreach (var obj in objectsToRemove)
        {
            if (activeHighlights.TryGetValue(obj, out Coroutine cor))
            {
                StopCoroutine(cor);
                activeHighlights.Remove(obj);
            }

            if (obj == null) 
                continue;

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
                return true;
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
                : Mathf.MoveTowards(rimRange, 0.01f, Time.deltaTime);

            rimBlend = increasing
                ? Mathf.MoveTowards(rimBlend, targetRimBlend, Time.deltaTime)
                : Mathf.MoveTowards(rimBlend, 0.01f, Time.deltaTime);

            material.SetFloat(RimRange, rimRange);
            material.SetFloat(RimBlend, rimBlend);

            if (increasing && Mathf.Approximately(rimRange, targetRimRange) && Mathf.Approximately(rimBlend, targetRimBlend))
                increasing = false;
            else if (!increasing && Mathf.Approximately(rimRange, 0.01f) && Mathf.Approximately(rimBlend, 0.01f))
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

    private void OnDrawGizmosSelected()
    {
        if (!showHighlightRange) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
