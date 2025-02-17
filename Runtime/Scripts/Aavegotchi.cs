using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class Aavegotchi : MonoBehaviour
{
    [SerializeField] private AavegtochiWearable_SmartconCombo KnightArmor;
    [SerializeField] private AavegtochiWearable_SmartconCombo Farmer;
    [SerializeField] private AavegtochiWearable_SmartconCombo RaverGirl;
    [SerializeField] private AavegtochiWearable_SmartconCombo Scientist;
    [SerializeField] private AavegtochiWearable_SmartconCombo Soldier;
    [SerializeField] private AavegtochiWearable_SmartconCombo Witch;

    [SerializeField] private Aavegotchi_CollateralsManager CollateralManager;

    [SerializeField] private GameObject EyeRoot;
    [SerializeField] private GameObject CollateralRoot;

    private Dictionary<EAavegotchiCombo, AavegtochiWearable_SmartconCombo> ComboDictionary = new Dictionary<EAavegotchiCombo, AavegtochiWearable_SmartconCombo>();

    private EAavegotchiCombo CurrentConfigedCombo = EAavegotchiCombo.None;

    public void Awake()
    {
        ComboDictionary.Add(EAavegotchiCombo.Armor, KnightArmor);
        ComboDictionary.Add(EAavegotchiCombo.Farmer, Farmer);
        ComboDictionary.Add(EAavegotchiCombo.RaverGirl, RaverGirl);
        ComboDictionary.Add(EAavegotchiCombo.Soldier, Soldier);
        ComboDictionary.Add(EAavegotchiCombo.Witch, Witch);
    }

    //--------------------------------------------------------------------------------------------------
    public Color GetOutlineColor()
    {
        if (CurrentConfigedCombo == EAavegotchiCombo.None)
        {
            return CollateralManager.GetData(ECollateral.Eth).PrimaryColor;
        }
        return CollateralManager.GetData(ComboDictionary[CurrentConfigedCombo].CollateralID).PrimaryColor;
    }

    //--------------------------------------------------------------------------------------------------
    public void SetupForCombo(EAavegotchiCombo newCombo)
    {
        if (CurrentConfigedCombo == newCombo)
        {
            return;
        }

        if (CurrentConfigedCombo != EAavegotchiCombo.None)
        {
            UnequipCombo(CurrentConfigedCombo);
        }

        CurrentConfigedCombo = newCombo;

        if (CurrentConfigedCombo != EAavegotchiCombo.None)
        {
            EquipCombo(CurrentConfigedCombo);
        }
        else
        {
            CollateralManager.SetupForCollateral(ECollateral.Eth);
        }
    }

    //--------------------------------------------------------------------------------------------------
    private void EquipCombo(EAavegotchiCombo combo)
    {
        var comboData = ComboDictionary[combo];

        CollateralManager.SetupForCollateral(comboData.CollateralID);

        comboData.gameObject.SetActive(true);

        foreach (var attachment in comboData.HandAttachments)
        {
            attachment.SetActive(true);
        }

        EyeRoot.SetActive(comboData.IsEyesEnabled);
        CollateralRoot.SetActive(comboData.IsCollateralEnabled);
    }

    //--------------------------------------------------------------------------------------------------
    private void UnequipCombo(EAavegotchiCombo combo)
    {
        var comboData = ComboDictionary[combo];
        foreach (var attachment in comboData.HandAttachments)
        {
            attachment.SetActive(false);
        }

        comboData.gameObject.SetActive(false);

        EyeRoot.SetActive(true);
        CollateralRoot.SetActive(true);
    }
}
