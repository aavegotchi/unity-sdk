using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class Aavegotchi_WearableLoader : MonoBehaviour
{
    [SerializeField]
    public Transform RootBone;

    [SerializeField]
    private SkinnedMeshRenderer MainBodyRenderer;

    [Header("Body")]
    [SerializeField] private Transform BodySpineRoot;

    [Header("Left Hand Sockets")]
    [SerializeField] private Transform LeftHand_Melee;
    [SerializeField] private Transform LeftHand_Shield;
    [SerializeField] private Transform LeftHand_Grenade;
    [SerializeField] private Transform LeftHand_Ranged;

    [Header("Right Hand Sockets")]
    [SerializeField] private Transform RightHand_Melee;
    [SerializeField] private Transform RightHand_Shield;
    [SerializeField] private Transform RightHand_Grenade;
    [SerializeField] private Transform RightHand_Ranged;

    [Header("Pet Socket")]
    [SerializeField] public Transform PetTransform;
    [SerializeField] public Transform PetBackTransform;

    [Header("Face Mouth Socket")]
    [SerializeField] public Transform MouthRoot;
    [SerializeField] public Transform DefaultSmile;

    [Header("Special Attachment Points")]
    public List<AavegotchiAttachmentPoint> AttachmentPoints = new List<AavegotchiAttachmentPoint>();

    public Dictionary<EWearableSlot, Aavegotchi_Wearable> LoadedWearables = new Dictionary<EWearableSlot, Aavegotchi_Wearable>();

    private int LastLoadedSkinID = 0;
    private List<int> PendingLoads = new List<int>();
    private Action OnWearablesUpdatedCB = null;
    private bool _isBeingDestroyed = false;

    //--------------------------------------------------------------------------------------------------
    private void OnDestroy()
    {
        _isBeingDestroyed = true;
        UnloadAllWearables(true);
    }

    //--------------------------------------------------------------------------------------------------
    public void UpdateWearables(Aavegotchi_Data data, IList<IResourceLocation> wearableAssetList, Action onWearablesUpdated)
    {
        // Make sure we cleanup any old attempt at loading
        if (OnWearablesUpdatedCB != null)
        {
            PendingLoads.Clear();
            OnWearablesUpdatedCB.Invoke();
            OnWearablesUpdatedCB = null;
        }

        OnWearablesUpdatedCB = onWearablesUpdated;
        LastLoadedSkinID = data.SkinID;
        UpdateSlot(data.Body_WearableID, wearableAssetList, EWearableSlot.Body, data.SkinID);
        UpdateSlot(data.Face_WearableID, wearableAssetList, EWearableSlot.Face, data.SkinID);
        UpdateSlot(data.Eyes_WearableID, wearableAssetList, EWearableSlot.Eyes, data.SkinID);
        UpdateSlot(data.Head_WearableID, wearableAssetList, EWearableSlot.Head, data.SkinID);
        UpdateSlot(data.HandLeft_WearableID, wearableAssetList, EWearableSlot.Hand_left, data.SkinID);
        UpdateSlot(data.HandRight_WearableID, wearableAssetList, EWearableSlot.Hand_right, data.SkinID);
        UpdateSlot(data.Pet_WearableID, wearableAssetList, EWearableSlot.Pet, data.SkinID);

        if (PendingLoads.Count == 0 && OnWearablesUpdatedCB != null)
        {
            OnWearablesUpdatedCB.Invoke();
            OnWearablesUpdatedCB = null;
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UpdateSlot(int wearableID, IList<IResourceLocation> wearableAssetList, EWearableSlot slotID, int skinID = 0)
    {
        if (wearableID == 0)
        {
            // If this slot should be empty, but we have a wearable loaded for it, unload it
            if (LoadedWearables.ContainsKey(slotID))
            {
                UnloadWearable(LoadedWearables[slotID]);
                LoadedWearables.Remove(slotID);
            }
        }
        else
        {
            if (LoadedWearables.ContainsKey(slotID) &&
                (LoadedWearables[slotID].WearableID != wearableID || LoadedWearables[slotID].SkinID != skinID))
            {
                // Unload the Addressable asset
                UnloadWearable(LoadedWearables[slotID]);
                LoadedWearables.Remove(slotID);
            }

            if (!LoadedWearables.ContainsKey(slotID))
            {
                LoadWearable(wearableID, wearableAssetList, slotID, skinID);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void LoadWearable(int wearableID, IList<IResourceLocation> wearableAssetList, EWearableSlot slot, int skinID = 0)
    {
        string addressablesKey = $"Wearable_Mesh_{wearableID}";

        if (skinID > 0)
        {
            addressablesKey = $"Wearable_Mesh_{wearableID}_{skinID}";
        }

        IResourceLocation targetLocation = null;

        foreach (var resourceLocation in wearableAssetList)
        {
            if (resourceLocation.PrimaryKey.Equals(addressablesKey))
            {
                targetLocation = resourceLocation;
                break;
            }
        }

        if (targetLocation == null)
        {
            Debug.LogWarning($"Skipping loading of wearable with ID: {wearableID} because its resource was not found");
            return;
        }

        PendingLoads.Add(wearableID);

        try
        {
            Addressables.LoadAssetAsync<GameObject>(targetLocation).Completed += (task) =>
            {
                if (task.Result != null)
                {
                    if(_isBeingDestroyed)
                        return;

                    // This will occur if in QUICK succession we load two different wearables for the same slot.
                    // The first may finish after we requested the 2nd. PendingLoads should contain the latest source of truth
                    // on which wearables we actually want.
                    if (!PendingLoads.Contains(wearableID) || skinID != LastLoadedSkinID)
                    {
                        return;
                    }

                    // Its possible from race condition for existing wearable to be loaded in the meantime
                    // that needs to be cleared out.
                    if (LoadedWearables.ContainsKey(slot))
                    {
                        if (LoadedWearables[slot].WearableID != wearableID || LoadedWearables[slot].SkinID != LastLoadedSkinID)
                        {
                            UnloadWearable(LoadedWearables[slot]);
                            LoadedWearables.Remove(slot);
                        }
                        else // The correct wearable is already loaded, so no need to make more
                        {
                            return;
                        }
                    }

                    var instance = Instantiate(task.Result, transform);
                    var loadedWearable = instance.GetComponent<Aavegotchi_Wearable>();
                    if (loadedWearable != null)
                    {
                        FixLayer(instance.transform, gameObject.layer);
                        LoadedWearables[slot] = loadedWearable;
                        loadedWearable.EquippedSlot = slot;
                        loadedWearable.Initialize(this, slot, MainBodyRenderer, () =>
                        {
                            if (loadedWearable.EquippedSlot == EWearableSlot.Hand_left && !loadedWearable.IsSkinnedWeapon)
                            {
                                if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Shield)
                                {
                                    loadedWearable.transform.SetParent(LeftHand_Shield, false);
                                }
                                else if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Grenade)
                                {
                                    loadedWearable.transform.SetParent(LeftHand_Grenade, false);
                                }
                                else if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Ranged)
                                {
                                    loadedWearable.transform.SetParent(LeftHand_Ranged, false);
                                }
                                else // if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Melee)
                                {
                                    loadedWearable.transform.SetParent(LeftHand_Melee, false);
                                }
                            }
                            else if (loadedWearable.EquippedSlot == EWearableSlot.Hand_right && !loadedWearable.IsSkinnedWeapon)
                            {
                                if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Shield)
                                {
                                    loadedWearable.transform.SetParent(RightHand_Shield, false);
                                }
                                else if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Grenade)
                                {
                                    loadedWearable.transform.SetParent(RightHand_Grenade, false);
                                }
                                else if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Ranged)
                                {
                                    loadedWearable.transform.SetParent(RightHand_Ranged, false);
                                }
                                else // if (loadedWearable.WearableTypeHint == EWearableTypeHint.Hand_Melee)
                                {
                                    loadedWearable.transform.SetParent(RightHand_Melee, false);
                                }
                            }
                            else if (loadedWearable.EquippedSlot == EWearableSlot.Pet)
                            {
                                if (loadedWearable.WearableTypeHint == EWearableTypeHint.Pet_Back)
                                {
                                    loadedWearable.transform.SetParent(PetBackTransform, false);
                                }
                                else //if(loadedWearable.WearableTypeHint == EWearableTypeHint.Pet)
                                {
                                    loadedWearable.transform.SetParent(PetTransform, false);
                                }
                            }
                            else if (loadedWearable.EquippedSlot == EWearableSlot.Face && loadedWearable.WearableTypeHint == EWearableTypeHint.Face_Mouth)
                            {
                                loadedWearable.transform.SetParent(MouthRoot, false);
                                DefaultSmile.gameObject.SetActive(false);
                            }
                            else if (loadedWearable.EquippedSlot == EWearableSlot.Body && loadedWearable.WearableTypeHint == EWearableTypeHint.Body_Spine)
                            {
                                loadedWearable.transform.SetParent(BodySpineRoot, false);
                            }

                            PendingLoads.Remove(wearableID);

                            if (PendingLoads.Count == 0 && OnWearablesUpdatedCB != null)
                            {
                                OnWearablesUpdatedCB.Invoke();
                                OnWearablesUpdatedCB = null;
                            }
                        });
                    }
                    else
                    {
                        PendingLoads.Remove(wearableID);

                        if (PendingLoads.Count == 0 && OnWearablesUpdatedCB != null)
                        {
                            OnWearablesUpdatedCB.Invoke();
                            OnWearablesUpdatedCB = null;
                        }

                        Destroy(instance);
                        Debug.LogError($"Failed to get Aavegotchi_Wearable script when loading wearable with ID: {wearableID}");
                    }
                }
                else
                {
                    PendingLoads.Remove(wearableID);

                    if (PendingLoads.Count == 0 && OnWearablesUpdatedCB != null)
                    {
                        OnWearablesUpdatedCB.Invoke();
                        OnWearablesUpdatedCB = null;
                    }

                    Debug.LogError($"Failed to load wearable with ID: {wearableID}");
                }
            };
        }
        catch (InvalidKeyException)
        {
            Debug.LogWarning($"Failed to load wearble with ID: {wearableID}");

            PendingLoads.Remove(wearableID);

            if (PendingLoads.Count == 0 && OnWearablesUpdatedCB != null)
            {
                OnWearablesUpdatedCB.Invoke();
                OnWearablesUpdatedCB = null;
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void FixLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform sub in root)
        {
            FixLayer(sub, layer);
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void UnloadWearable(Aavegotchi_Wearable wearable, bool beingDestroyed = false)
    {
        if (!beingDestroyed)
        {
            if (wearable.EquippedSlot == EWearableSlot.Face && wearable.WearableTypeHint == EWearableTypeHint.Face_Mouth)
            {
                DefaultSmile.gameObject.SetActive(true);
            }
        }

        Addressables.ReleaseInstance(wearable.gameObject);
        Destroy(wearable.gameObject);
    }

    //--------------------------------------------------------------------------------------------------
    private void UnloadAllWearables(bool beingDestroyed)
    {
        foreach (var loadedWearable in LoadedWearables.Values)
        {
            UnloadWearable(loadedWearable, beingDestroyed);
        }

        LoadedWearables.Clear();
    }
}
