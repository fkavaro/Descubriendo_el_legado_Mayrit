using System;
using System.Collections.Generic;
using UnityEngine;

public class Workplace : AAssignedBuilding
{
    #region EDITOR PROPERTIES
    [Header("Workplace Properties")]
    [SerializeField] List<Spot> _workSpots;
    [SerializeField] bool _isOpen = false;
    public bool IsWorkplaceOpen
    {
        get { return _isOpen; }
        set { _isOpen = value; }
    }
    #endregion

    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        _townManager.RegisterWorkplace(this);
    }

    public override void UnregisterBuilding()
    {
        _townManager.UnregisterWorkplace(this);
    }

    public override void Reassign(List<Villager> employees)
    {
        _townManager.ReassignEmployees(this, employees);
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
