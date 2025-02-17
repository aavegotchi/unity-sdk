using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class Aavegotchi_Base : MonoBehaviour
{
    [SerializeField] private Aavegotchi_Data CurrentData;

    [SerializeField] private bool Refresh = true;

    [Header("Unity Object Controllers")]
    [SerializeField] private Aavegotchi_Eyes Eyes;

    [SerializeField] public Aavegotchi_Collaterals Collaterals;

    [SerializeField] public Aavegotchi_WearableLoader Wearable_Loader;

    [SerializeField] public GameObject CheeksRoot;

    [SerializeField] private Animator GotchiAnimator;

    [SerializeField] private int ChampionID = 0;
    private int _LastSetChampion = 0;

    [SerializeField] private bool isRandomSeedActive = false;

    [Header("change color of gotchi!")]
    [SerializeField] public List<SkinnedMeshRenderer> ColorableRenderers = new List<SkinnedMeshRenderer>();
    [SerializeField] public SkinnedMeshRenderer BodyWithMouthRenderer;
    [SerializeField] public Material DefaultGotchiMat;

    IList<IResourceLocation> WearableAssetList = null;

    private AavegotchiChampionsList ChampionsList = null;

    private const string RandomSeed = "RandomSeed";
    private const string MeleeHandsFlag = "MeleeHands";
    private const string GrenadeHandsFlag = "GrenadeHands";
    private const string ShieldHandsFlag = "ShieldHands";
    private const string RangedHandsFlag = "RangedHands";
    private const string ChampionListKey = "AavegotchiSOHolder";

    //--------------------------------------------------------------------------------------------------
    void Update()
    {
        if (isRandomSeedActive)
        {
            GotchiAnimator.SetFloat(RandomSeed, UnityEngine.Random.Range(0, 1001) / 1000.0f);

            foreach (var (key, wearable) in Wearable_Loader.LoadedWearables)
            {
                if (wearable.CharacterAnimator != null)
                {
                    wearable.CharacterAnimator.SetFloat(RandomSeed, UnityEngine.Random.Range(0, 1001) / 1000.0f);
                }
                foreach (var additionalAnimator in wearable.AdditionalAnimators)
                {
                    additionalAnimator.SetFloat(RandomSeed, UnityEngine.Random.Range(0, 1001) / 1000.0f);
                }
            }
        }

        if (Refresh)
        {
            Refresh = false;

            UpdateForData(CurrentData, force:true);
        }

        LoadChampion(ChampionID);
    }

    //--------------------------------------------------------------------------------------------------
    public void LoadChampion(int championID, Action OnDataCompleted = null)
    {
        if (_LastSetChampion == championID)
        {
            return;
        }

        ChampionID = championID;
        _LastSetChampion = championID;

        LoadChampionAsync(championID, OnDataCompleted);
    }

    //--------------------------------------------------------------------------------------------------
    private void LoadChampionAsync(int championID, Action OnDataCompleted = null)
    {
        Addressables.InitializeAsync().Completed += (task) =>
        {
            if (ChampionsList == null)
            {
                gameObject.SetActive(false);

                if (WearableAssetList == null)
                {
                    Addressables.LoadResourceLocationsAsync("Wearable").Completed += (task) =>
                    {
                        if (task.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (task.Result != null)
                            {
                                WearableAssetList = task.Result;

                                LoadChamionsData(championID, OnDataCompleted);
                            }
                        }
                    };
                }
                else
                {
                    LoadChamionsData(championID, OnDataCompleted);
                }
            }
            else
            {
                var championData = ChampionsList.GetChampionForID(championID);

                if (championData != null)
                {
                    UpdateForData(championData.Data, OnDataCompleted);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        };
    }

    //--------------------------------------------------------------------------------------------------
    private void LoadChamionsData(int championID, Action OnDataCompleted = null)
    {
        var championsResourceLocation = GetWearableResource(ChampionListKey);

        if (championsResourceLocation != null)
        {
            Addressables.LoadAssetAsync<GameObject>(ChampionListKey).Completed += (task) =>
            {
                if (task.Result != null)
                {
                    ChampionsList = task.Result.GetComponent<AavegotchiSOHolder>().ChampionList;

                    var championData = ChampionsList.GetChampionForID(championID);

                    if (championData != null)
                    {
                        UpdateForData(championData.Data, OnDataCompleted);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError("Failed to load AavegotchiChampionsList");
                }
            };
        }
    }

    //--------------------------------------------------------------------------------------------------
    private IResourceLocation GetWearableResource(string key)
    {
        foreach (var location in WearableAssetList)
        {
            if (location.PrimaryKey.Contains(key))
            {
                return location;
            }
        }

        return null;
    }

    //--------------------------------------------------------------------------------------------------
    public void UpdateForData(Aavegotchi_Data newData, Action OnDataCompleted = null, bool force = false)
    {
        if (!force && CurrentData.Equals(newData))
        {
            OnDataCompleted?.Invoke();
            return;
        }

        CurrentData = new Aavegotchi_Data(newData);

        if (WearableAssetList == null)
        {
            Addressables.InitializeAsync().Completed += (task) =>
            {
                Addressables.LoadResourceLocationsAsync("Wearable").Completed += (task) =>
                {
                    if (task.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (task.Result != null)
                        {
                            WearableAssetList = task.Result;

                            UpdateForData(newData, OnDataCompleted, force:true);
                        }
                    }
                };
            };

            return;
        }

        Collaterals.UpdateConfiguration(CurrentData.CollateralType, CurrentData.EyeColor, Eyes);
        Eyes.UpdateConfiguration(CurrentData.EyeShape, CurrentData.CollateralType, CurrentData.EyeColor, Collaterals);

        bool pending = true;
        Wearable_Loader.UpdateWearables(newData, WearableAssetList, () =>
        {
            pending = false;

            if (this != null && gameObject != null) { 
                gameObject.SetActive(true);
                UpdateWearableLinks();
            }
            OnDataCompleted?.Invoke();
        });

        if (pending)
        {
            if (this != null && gameObject != null)
                gameObject.SetActive(false);
        }
        else
        {
            if (this != null && gameObject != null)
                gameObject.SetActive(true);
            OnDataCompleted?.Invoke();
        }
    }

    //--------------------------------------------------------------------------------------------------
    public Color GetOutlineColor()
    {
        return Collaterals.GetData(CurrentData.CollateralType).PrimaryColor;
    }

    //--------------------------------------------------------------------------------------------------
    // Certain aspects change based on the wearables we were for example, some wearables disable the
    // Collateral rendering. This function updates all of this
    //--------------------------------------------------------------------------------------------------
    private void UpdateWearableLinks()
    {
        bool collateralEnabled = true;
        bool eyesEnabled = true;
        bool disableEyeWearables = false;

        bool disableCheeks = false;

        Material targetMaterial = DefaultGotchiMat;

        foreach (var (key, value) in Wearable_Loader.LoadedWearables)
        {
            if (value.DisableCollateral)
            {
                collateralEnabled = false;
            }

            if (value.DisableEyes)
            {
                eyesEnabled = false;
            }

            if (value.DisableEyeWearables)
            {
                disableEyeWearables = true;
            }

            if (value.GotchiMat != null)
            {
                targetMaterial = value.GotchiMat;
            }

            if (value.WearableTypeHint == EWearableTypeHint.Face_Cheeks ||
                value.DisableCheeks)
            {
                disableCheeks = true;
            }
        }

        if (Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Eyes))
        {
            Wearable_Loader.LoadedWearables[EWearableSlot.Eyes].gameObject.SetActive(!disableEyeWearables);
        }

        foreach (var colorableRenderer in ColorableRenderers)
        {
            colorableRenderer.sharedMaterial = targetMaterial;
        }

        var sharedMaterials = BodyWithMouthRenderer.sharedMaterials;
        sharedMaterials[0] = targetMaterial;
        BodyWithMouthRenderer.sharedMaterials = sharedMaterials;

        Eyes.gameObject.SetActive(eyesEnabled);
        Collaterals.gameObject.SetActive(collateralEnabled);

        UpdateAnimatorFlag(MeleeHandsFlag, EWearableTypeHint.Hand_Melee);
        UpdateAnimatorFlag(GrenadeHandsFlag, EWearableTypeHint.Hand_Grenade);
        UpdateAnimatorFlag(ShieldHandsFlag, EWearableTypeHint.Hand_Shield);
        UpdateAnimatorFlag(RangedHandsFlag, EWearableTypeHint.Hand_Ranged);

        CheeksRoot.SetActive(!disableCheeks);

        // Shoulders logic for weapons like energy gun
        if (Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Body))
        {
            var leftSleeveDisabled = Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Hand_left) && Wearable_Loader.LoadedWearables[EWearableSlot.Hand_left].DisableShoulder;
            var rightSleeveDisabled = Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Hand_right) && Wearable_Loader.LoadedWearables[EWearableSlot.Hand_right].DisableShoulder;

            Wearable_Loader.LoadedWearables[EWearableSlot.Body].UpdateSleeves(leftSleeveDisabled, rightSleeveDisabled);
        }
    }

    //--------------------------------------------------------------------------------------------------
    // 0 = both hands
    // 1 = favor left hand
    // 2 = favor right hand
    //--------------------------------------------------------------------------------------------------
    private void UpdateAnimatorFlag(string animatorKey, EWearableTypeHint hintToVerifty)
    {
        int flagValue = 0;
        var leftHandWearabe = Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Hand_left) ? Wearable_Loader.LoadedWearables[EWearableSlot.Hand_left] : null;
        var rightHandWearable = Wearable_Loader.LoadedWearables.ContainsKey(EWearableSlot.Hand_right) ? Wearable_Loader.LoadedWearables[EWearableSlot.Hand_right] : null;

        if (leftHandWearabe != null && leftHandWearabe.WearableTypeHint == hintToVerifty && (rightHandWearable == null || rightHandWearable.WearableTypeHint != hintToVerifty))
        {
            flagValue = 1;
        }
        else if (rightHandWearable != null && rightHandWearable.WearableTypeHint == hintToVerifty && (leftHandWearabe == null || leftHandWearabe.WearableTypeHint != hintToVerifty))
        {
            flagValue = 2;
        }
        else if (rightHandWearable != null && leftHandWearabe == null)
        {
            flagValue = 1;
        }
        else if (leftHandWearabe != null && rightHandWearable == null)
        {
            flagValue = 2;
        }

        GotchiAnimator.SetInteger(animatorKey, flagValue);
    }
}
