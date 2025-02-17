using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum EWearableSlot
{
    Body,
    Face,
    Eyes,
    Head,
    Hand_left,
    Hand_right,
    Pet,
}

public enum EWearableTypeHint
{
    Body,
    Face,
    Eyes,
    Head,
    Hand_Melee,
    Hand_Shield,
    Hand_Grenade,
    Hand_Ranged,
    Hand_Other,
    Pet,
    Pet_Back,
    Face_Mouth,

    Body_Spine,
    Face_Cheeks
}

public enum EWearableType
{
    None,
    Melee_Basic,
    Melee_Long_Range,
    Melee_High_Rate,
    Melee_Pierce,
    Ranged_Basic,
    Ranged_FallOff,
    Ranged_Magical,
    Ranged_Sniper,
    Basic_Grenade,
    Impact_Grenade,
    Shield,
}

public class Aavegotchi_Wearable : MonoBehaviour
{
    [Header("Must Match!")]
    [SerializeField]
    public int WearableID;

    [SerializeField]
    public int SkinID = 0;

    [Header("[OPTIONAL] Load a material addressable dynmically to apply to mesh renderers")]
    [SerializeField]
    private string MaterialName;

    [Header("[OPTIONAL] used by wearables like pets to control their animations")]
    [SerializeField]
    public Animator CharacterAnimator = null;
    [SerializeField]
    public List<Animator> AdditionalAnimators = new List<Animator>();

    [Header("Required for fixing bones")]
    [SerializeField]
    private List<SkinnedMeshRenderer> SkinnedMeshRenderers;

    [Header("[OPTIONAL] used for dynamic materials")]
    [SerializeField]
    private List<MeshRenderer> MeshRenderers;

    [Header("Wearable types")]
    [SerializeField]
    public EWearableTypeHint WearableTypeHint;

    [SerializeField]
    public EWearableType WearableType;

    [SerializeField]
    public EWearableSlot EquippedSlot;

    [Header("[OPTIONAL] Used primarily for body wearables")]
    [SerializeField]
    public GameObject LeftShoulder;

    [SerializeField]
    public GameObject RightShoulder;

    [Header("[OPTIONAL] Hand Transform overrides")]
    [SerializeField]
    public Transform LeftHandTransform;

    [SerializeField]
    public Transform RightHandTransform;

    [Header("Base Gotchi modifications")]
    [SerializeField]
    public bool DisableCollateral = false;

    [SerializeField]
    public bool DisableEyes = false;

    [SerializeField]
    public bool DisableEyeWearables = false;

    [SerializeField]
    public bool IsSkinnedWeapon = false;

    [SerializeField]
    public bool DisableCheeks = false;

    [SerializeField]
    public bool DisableShoulder = false;
    private bool _isBeingDestroyed = false;

    [Header("OPTIONAL, COLORS!")]
    [SerializeField]
    public Material GotchiMat = null;

    [Header("OPTIONAL, Spawn and attach!")]
    [SerializeField]
    public List<WearableAttachment> Attachments = new List<WearableAttachment>();

    private List<GameObject> SpawnedObjects = new List<GameObject>();

    //--------------------------------------------------------------------------------------------------
    public void OnDestroy()
    {
        _isBeingDestroyed = true;

        foreach (var spawnedObj in SpawnedObjects)
        {
            Destroy(spawnedObj);
        }
        SpawnedObjects.Clear();
    }

    //--------------------------------------------------------------------------------------------------
    public void Initialize(Aavegotchi_WearableLoader loader, EWearableSlot targetSlot, SkinnedMeshRenderer mainBodyRenderer, Action onLoadedCompletedCB)
    {
        EquippedSlot = targetSlot;

        UpdateSkinning(loader, targetSlot, mainBodyRenderer);

        InitializeSpawnedObjects(loader.AttachmentPoints);

        UpdateHandSlots();

        // Loading material. Watchout this function handles the onLoadedCompletedCB based on whether there is a dynamic wearable to wear or not.
        LoadExtraMaterials(onLoadedCompletedCB);
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateSkinning(Aavegotchi_WearableLoader loader, EWearableSlot targetSlot, SkinnedMeshRenderer mainBodyRenderer)
    {
        foreach (var skinnedMeshRenderer in SkinnedMeshRenderers)
        {
            skinnedMeshRenderer.bones = mainBodyRenderer.bones;
            skinnedMeshRenderer.rootBone = loader.RootBone;
        }

        if (IsSkinnedWeapon)
        {
            // Since theres only 1 right now, it seems reasonable to assume skinned mesh renderer on indexes means which arm its for.
            if (SkinnedMeshRenderers.Count == 2)
            {
                SkinnedMeshRenderers[0].gameObject.SetActive(targetSlot == EWearableSlot.Hand_left);
                SkinnedMeshRenderers[1].gameObject.SetActive(targetSlot == EWearableSlot.Hand_right);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void InitializeSpawnedObjects(List<AavegotchiAttachmentPoint> attachmentPoints)
    {
        foreach (var toSpawn in Attachments)
        {
            foreach (var attachmentPoint in attachmentPoints)
            {
                if (attachmentPoint.Name == toSpawn.Name)
                {
                    var spawnedObj = Instantiate(toSpawn.ToAttach, attachmentPoint.Target);
                    spawnedObj.layer = attachmentPoint.Target.gameObject.layer;
                    SpawnedObjects.Add(spawnedObj);
                    break;
                }
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateHandSlots()
    {
        if (EquippedSlot == EWearableSlot.Hand_left && LeftHandTransform != null)
        {
            foreach (var meshRenderer in MeshRenderers)
            {
                meshRenderer.transform.SetParent(LeftHandTransform, false);
            }
        }
        else if (EquippedSlot == EWearableSlot.Hand_right && RightHandTransform != null)
        {
            foreach (var meshRenderer in MeshRenderers)
            {
                meshRenderer.transform.SetParent(RightHandTransform, false);
            }
        }
    }

    // --------------------------------------------------------------------------------------------------
    public void LoadExtraMaterials(Action onLoadedCompletedCB = null)
    {
        if (string.IsNullOrEmpty(MaterialName))
        {
            onLoadedCompletedCB.Invoke();
            return;
        }

        gameObject.SetActive(false);

        // Loading material
        Addressables.LoadAssetAsync<Material>($"Wearable_Mat_{MaterialName}").Completed += (task) =>
        {
            if (task.Result != null)
            {
                if (_isBeingDestroyed) return;
                foreach (var skinnedMeshRenderer in SkinnedMeshRenderers)
                {
                    // Check if the skinnedMeshRenderer is null or has been destroyed
                    if (skinnedMeshRenderer == null)
                        continue;
                    if (skinnedMeshRenderer.materials.Length > 1)
                    {
                        var materialsCopy = skinnedMeshRenderer.materials;

                        for (int i = 0; i < materialsCopy.Length; ++i)
                        {
                            materialsCopy[i] = task.Result;
                        }

                        skinnedMeshRenderer.materials = materialsCopy;
                    }
                    else
                    {
                        skinnedMeshRenderer.material = task.Result;
                    }
                }

                foreach (var meshRenderer in MeshRenderers)
                {
                    if (meshRenderer == null)
                        continue;
                    if (meshRenderer.materials.Length > 1)
                    {
                        var materialsCopy = meshRenderer.materials;

                        for (int i = 0; i < materialsCopy.Length; ++i)
                        {
                            materialsCopy[i] = task.Result;
                        }

                        meshRenderer.materials = materialsCopy;
                    }
                    else
                    {
                        meshRenderer.material = task.Result;
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to load material: {MaterialName} - for wearable with ID: {WearableID}");
            }

            if ( !_isBeingDestroyed && this != null && gameObject != null)
                gameObject.SetActive(true);

            onLoadedCompletedCB?.Invoke();
        };
    }

    //--------------------------------------------------------------------------------------------------
    internal void UpdateSleeves(bool leftSleeveDisabled, bool rightSleeveDisabled)
    {
        if (_isBeingDestroyed) return;
        if (LeftShoulder != null && LeftShoulder.gameObject != null)
        {
            LeftShoulder.gameObject.SetActive(!leftSleeveDisabled);
        }

        if (RightShoulder != null && RightShoulder.gameObject != null)
        {
            RightShoulder.gameObject.SetActive(!rightSleeveDisabled);
        }
    }
}