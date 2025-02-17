using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Aavegotchi_Data
{
    public int HauntID;
    public ECollateral CollateralType;
    public EEyeShape EyeShape;
    public EEyeColor EyeColor;

    public int Body_WearableID = 0;
    public int Face_WearableID = 0;
    public int Eyes_WearableID = 0;
    public int Head_WearableID = 0;
    public int Pet_WearableID = 0;
    public int HandLeft_WearableID = 0;
    public int HandRight_WearableID = 0;

    public int SkinID = 0; // Used by Gotchi Guardians to handle guardian skins.

    //--------------------------------------------------------------------------------------------------
    // Copy constructor
    //--------------------------------------------------------------------------------------------------
    public Aavegotchi_Data(Aavegotchi_Data other)
    {
        HauntID = other.HauntID;
        CollateralType = other.CollateralType;
        EyeShape = other.EyeShape;
        EyeColor = other.EyeColor;
        Body_WearableID = other.Body_WearableID;
        Face_WearableID = other.Face_WearableID;
        Eyes_WearableID = other.Eyes_WearableID;
        Head_WearableID = other.Head_WearableID;
        Pet_WearableID = other.Pet_WearableID;
        HandLeft_WearableID = other.HandLeft_WearableID;
        HandRight_WearableID = other.HandRight_WearableID;
        SkinID = other.SkinID;
    }

    //--------------------------------------------------------------------------------------------------
    public Aavegotchi_Data()
    {

    }

    //--------------------------------------------------------------------------------------------------
    public bool Equals(Aavegotchi_Data other)
    {
        if (HauntID != other.HauntID)
            return false;

        if (CollateralType != other.CollateralType)
            return false;

        if (EyeShape != other.EyeShape)
            return false;

        if (EyeColor != other.EyeColor)
            return false;

        if (Body_WearableID != other.Body_WearableID)
            return false;

        if (Face_WearableID != other.Face_WearableID)
            return false;

        if (Eyes_WearableID != other.Eyes_WearableID)
            return false;

        if (Head_WearableID != other.Head_WearableID)
            return false;

        if (Pet_WearableID != other.Pet_WearableID)
            return false;

        if (HandLeft_WearableID != other.HandLeft_WearableID)
            return false;

        if (HandRight_WearableID != other.HandRight_WearableID)
            return false;

        if (SkinID != other.SkinID)
            return false;

        return true;
    }
}
