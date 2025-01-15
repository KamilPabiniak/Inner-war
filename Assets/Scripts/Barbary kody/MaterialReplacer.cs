using UnityEngine;

public class MaterialReplacer : MonoBehaviour
{
    [SerializeField] private Material newShaderMaterial;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Material oldMaterial = renderer.material;

        Material instanceMaterial = new Material(newShaderMaterial);

        if (oldMaterial.HasProperty("_BaseMap") && instanceMaterial.HasProperty("_BaseMap"))
        {
            instanceMaterial.SetTexture("_BaseMap", oldMaterial.GetTexture("_BaseMap"));
        }
        else if (oldMaterial.HasProperty("_MainTex") && instanceMaterial.HasProperty("_BaseMap"))
        {
            instanceMaterial.SetTexture("_BaseMap", oldMaterial.GetTexture("_MainTex"));
        }
        if (oldMaterial.HasProperty("_MetallicGlossMap") && instanceMaterial.HasProperty("_MetallicMap"))
        {
            instanceMaterial.SetTexture("_MetallicMap", oldMaterial.GetTexture("_MetallicGlossMap"));
        }

        if (oldMaterial.HasProperty("_BumpMap") && instanceMaterial.HasProperty("_NormalMap"))
        {
            instanceMaterial.SetTexture("_NormalMap", oldMaterial.GetTexture("_BumpMap"));
        }
        
        renderer.material = instanceMaterial;
    }
}
