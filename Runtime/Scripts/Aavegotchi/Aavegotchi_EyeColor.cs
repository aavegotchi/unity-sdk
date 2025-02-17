using System.Collections.Generic;
using UnityEngine;

public class Aavegotchi_EyeColor : MonoBehaviour
{
    [SerializeField] private int StakeColorMaterialIndex = 0;
    [SerializeField] private int EyeColorMaterialIndex = -1;

    [SerializeField] private List<Material> EyeColorMaterials = new List<Material>();

    //--------------------------------------------------------------------------------------------------
    public void UpdateEyeColors(Material stackMaterial, Material eyeColorMaterial, EEyeColor eyeColor)
    {
        if (gameObject.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
        {
            var materials = skinnedMeshRenderer.materials;

            if (StakeColorMaterialIndex >= 0 && StakeColorMaterialIndex < materials.Length)
            {
                materials[StakeColorMaterialIndex] = stackMaterial;
            }

            if (EyeColorMaterialIndex >= 0 && EyeColorMaterialIndex < materials.Length)
            {
                materials[EyeColorMaterialIndex] = eyeColorMaterial;
            }

            if (EyeColorMaterials.Count > 0)
            {
                materials[0] = EyeColorMaterials[(int)eyeColor];
            }

            skinnedMeshRenderer.materials = materials;
        }
    }
}
