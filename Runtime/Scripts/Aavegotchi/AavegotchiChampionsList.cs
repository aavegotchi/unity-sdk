using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AavegotchiChampionsList", menuName = "SO/Aavegotchi/Aavegotchi Champion List")]
[Serializable]
public class AavegotchiChampionsList : ScriptableObject
{
    [SerializeField] public List<Aavegotchi_Champion> Champions = new List<Aavegotchi_Champion>();

    //--------------------------------------------------------------------------------------------------
    public Aavegotchi_Champion GetChampionForID(int id)
    {
        foreach (var champion in Champions)
        {
            if (champion.UniqueID == id) return champion;
        }

        return null;
    }
}