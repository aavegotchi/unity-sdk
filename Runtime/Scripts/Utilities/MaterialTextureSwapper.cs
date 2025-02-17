using System.Collections.Generic;
using UnityEngine;

public class MaterialTextureSwapper : MonoBehaviour
{
    [SerializeField] private Material TargetMaterial;
    [SerializeField] private List<Texture2D> Textures;
    [SerializeField] private List<Texture2D> EmissionTextures; // New list for emission textures
    [SerializeField] private bool AnimateEmission = false; // New option to enable/disable emission animation
    [SerializeField] private float TimeToSwitch = 0.15f;
    private float CurrentTimer;
    private int CurrentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (AnimateEmission && EmissionTextures.Count != Textures.Count)
        {
            Debug.LogError("The number of emission textures must match the number of main textures.");
            AnimateEmission = false; // Disable emission animation to prevent errors
        }
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTimer += Time.deltaTime;

        if (CurrentTimer >= TimeToSwitch)
        {
            CurrentTimer -= TimeToSwitch;

            CurrentIndex = (CurrentIndex + 1) % Textures.Count;

            // Set the main texture
            TargetMaterial.mainTexture = Textures[CurrentIndex];

            // Set the emission texture if the option is enabled
            if (AnimateEmission)
            {
                TargetMaterial.SetTexture("_EmissionMap", EmissionTextures[CurrentIndex]);
            }
        }
    }
}
