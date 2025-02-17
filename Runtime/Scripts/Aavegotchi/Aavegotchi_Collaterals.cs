using System;
using System.Collections.Generic;
using UnityEngine;

public enum ECollateral
{
    Eth,
    Aave,
    Dai,
    Link,
    USDT,
    USDC,
    TUSD,
    Uni,
    Yfi,
    Polygon,
    wEth,
    wBTC
}

[Serializable]
public struct CollateralData
{
    [SerializeField] public string CollateralName;
    [SerializeField] public GameObject CollateralMesh;
    [SerializeField] public Material PrimaryMaterial;
    [SerializeField] public Material CheeksMaterial;
    [SerializeField] public Material InnerMouthMaterial;
    [ColorUsage(true, true)]
    [SerializeField] public Color PrimaryColor;
    [ColorUsage(true, true)]
    [SerializeField] public Color SecondaryColor;
    [ColorUsage(true, true)]
    [SerializeField] public Color CheeksColor;
}

public class Aavegotchi_Collaterals : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private List<SkinnedMeshRenderer> CheekRenderers = new List<SkinnedMeshRenderer>();
    [SerializeField] private List<SkinnedMeshRenderer> EmotionEyeRenderers = new List<SkinnedMeshRenderer>();
    [SerializeField] private List<SkinnedMeshRenderer> MouthRenderers = new List<SkinnedMeshRenderer>();
    [SerializeField] private SkinnedMeshRenderer InnerMouthRenderer;

    [Header("CollateralData")]
    [SerializeField] public CollateralData EthCollateral;
    [SerializeField] public CollateralData AaveCollateral;
    [SerializeField] public CollateralData DaiCollateral;
    [SerializeField] public CollateralData LinkCollateral;
    [SerializeField] public CollateralData USDTCollateral;
    [SerializeField] public CollateralData USDCCollateral;
    [SerializeField] public CollateralData TUSDCollateral;
    [SerializeField] public CollateralData UniCollateral;
    [SerializeField] public CollateralData YfiCollateral;
    [SerializeField] public CollateralData PolygonCollateral;
    [SerializeField] public CollateralData wEthCollateral;
    [SerializeField] public CollateralData wBTCCollateral;

    private Dictionary<ECollateral, CollateralData> CollateralDataMap = new Dictionary<ECollateral, CollateralData>();

    //--------------------------------------------------------------------------------------------------
    void Awake()
    {
        CollateralDataMap.Add(ECollateral.Eth, EthCollateral);
        CollateralDataMap.Add(ECollateral.Aave, AaveCollateral);
        CollateralDataMap.Add(ECollateral.Dai, DaiCollateral);
        CollateralDataMap.Add(ECollateral.Link, LinkCollateral);
        CollateralDataMap.Add(ECollateral.USDT, USDTCollateral);
        CollateralDataMap.Add(ECollateral.USDC, USDCCollateral);
        CollateralDataMap.Add(ECollateral.TUSD, TUSDCollateral);
        CollateralDataMap.Add(ECollateral.Uni, UniCollateral);
        CollateralDataMap.Add(ECollateral.Yfi, YfiCollateral);
        CollateralDataMap.Add(ECollateral.Polygon, PolygonCollateral);
        CollateralDataMap.Add(ECollateral.wEth, wEthCollateral);
        CollateralDataMap.Add(ECollateral.wBTC, wBTCCollateral);
    }

    //--------------------------------------------------------------------------------------------------
    public void UpdateConfiguration(ECollateral collateral, EEyeColor eyeColor, Aavegotchi_Eyes eyes)
    {
        foreach (var (key, value) in CollateralDataMap)
        {
            // Check if CollateralMesh is null or has been destroyed before accessing it
            if (value.CollateralMesh != null && value.CollateralMesh.gameObject != null)    
                value.CollateralMesh?.SetActive(key == collateral);
        }

        var collateralData = CollateralDataMap[collateral];

        foreach (var cheekRenderer in CheekRenderers)
        {
            if (cheekRenderer != null)
                cheekRenderer.material = collateralData.CheeksMaterial;
        }

        var targetMaterial = collateralData.PrimaryMaterial;

        switch (eyeColor)
        {
            case EEyeColor.Uncommon_Low:
                targetMaterial = eyes.EyeColor_UncommonLow_Mat;
                break;
            case EEyeColor.Uncommon_High:
                targetMaterial = eyes.EyeColor_UncommonHigh_Mat;
                break;
            case EEyeColor.Rare_Low:
                targetMaterial = eyes.EyeColor_RareLow_Mat;
                break;
            case EEyeColor.Rare_High:
                targetMaterial = eyes.EyeColor_RareHigh_Mat;
                break;
            case EEyeColor.Mythical_Low:
                targetMaterial = eyes.EyeColor_MythicalLow_Mat;
                break;
            case EEyeColor.Mythical_High:
                targetMaterial = eyes.EyeColor_MythicalHigh_Mat;
                break;

        }

        foreach (var emotionRenderer in EmotionEyeRenderers)
        {
            if(emotionRenderer != null)
                emotionRenderer.material = targetMaterial;
        }

        foreach (var mouthRenderer in MouthRenderers)
        {
            if (mouthRenderer != null)
                mouthRenderer.material = collateralData.PrimaryMaterial;
        }

        if (InnerMouthRenderer != null)
        {
            var sharedMaterials = InnerMouthRenderer.sharedMaterials;
            sharedMaterials[1] = collateralData.InnerMouthMaterial;
            InnerMouthRenderer.sharedMaterials = sharedMaterials;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public CollateralData GetData(ECollateral targetCollateral)
    {
        return CollateralDataMap[targetCollateral];
    }
}