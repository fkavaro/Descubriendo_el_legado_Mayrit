using System;
using System.Collections.Generic;
using UnityEngine;

public class Workplace : AAssignedBuilding
{
    #region EDITOR PROPERTIES
    [Header("Workplace Properties")]
    [SerializeField] List<Spot> _workSpots;
    [SerializeField] protected bool _isInterior = false;
    public bool _isOpen = false;

    public bool IsInterior => _isInterior;
    public List<Spot> WorkSpots => _workSpots;
    #endregion

    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        TownManager.RegisterWorkplace(this);
    }

    public override void UnregisterBuilding()
    {
        TownManager.UnregisterWorkplace(this);
    }

    public override void ReassignVillagers(List<Villager> employees)
    {
        TownManager.ReassignEmployees(this, employees);
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
