using System;
using System.Collections.Generic;
using UnityEngine;

public class House : AAssignedBuilding
{
    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.RegisterHouse(this);
    }

    public override void UnregisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.UnregisterHouse(this);
    }

    public override void Reassign(List<Villager> residents)
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.ReassignResidents(this, residents);
    }
    #endregion

    public void IncreaseCapacity(int increase)
    {
        _capacity += increase;
        var tm = TownManager.ExistingInstance;
        if (tm != null)
            tm.UpdatePopulation(increase);
    }
}
