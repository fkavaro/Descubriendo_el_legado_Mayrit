using System;
using System.Collections.Generic;
using UnityEngine;

public class House : AAssignedBuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        TownManager.RegisterHouse(this);
    }

    public override void UnregisterBuilding()
    {
        TownManager.UnregisterHouse(this);
    }

    public override void ReassignVillagers(List<Villager> residents)
    {
        TownManager.ReassignResidents(this, residents);
    }
    #endregion

    public override void IncreaseCapacity(int increase)
    {
        base.IncreaseCapacity(increase);
        TownManager.UpdatePopulation(increase);
    }
}
