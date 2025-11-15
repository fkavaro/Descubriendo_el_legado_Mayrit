using System;
using System.Collections.Generic;
using UnityEngine;

public class Workplace : AAssignedBuilding
{
    #region EDITOR PROPERTIES
    [Header("Workplace Properties")]
    [SerializeField] List<Spot> _workSpots;
    #endregion

    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.RegisterWorkplace(this);
    }

    public override void UnregisterBuilding()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.UnregisterWorkplace(this);
    }

    public override void Reassign(List<Villager> residents)
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.ReassignEmployees(this, residents);
    }
    #endregion

    #region PUBLIC METHODS
    public Spot GetRandomWorkingSpot()
    {
        if (_workSpots == null || _workSpots.Count == 0) return null;
        int randomIndex = UnityEngine.Random.Range(0, _workSpots.Count);
        return _workSpots[randomIndex];
    }
    #endregion
}
