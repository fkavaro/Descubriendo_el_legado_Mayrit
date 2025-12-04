using System;
using System.Collections.Generic;
using UnityEngine;

public class House : AAssignedBuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        _townManager.RegisterHouse(this);
    }

    public override void UnregisterBuilding()
    {
        _townManager.UnregisterHouse(this);
    }

    public override void Reassign(List<Villager> residents)
    {
        _townManager.ReassignResidents(this, residents);
    }
    #endregion

    public override void IncreaseCapacity(int increase)
    {
        base.IncreaseCapacity(increase);
        _townManager.UpdatePopulation(increase);
    }
}
