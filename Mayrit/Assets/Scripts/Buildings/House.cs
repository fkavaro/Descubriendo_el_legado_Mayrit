using System;
using System.Collections.Generic;
using UnityEngine;

public class House : ABuilding
{
    #region EDITOR PROPERTIES
    public int _householdSize = 1;
    [SerializeField] List<Villager> _residents = new();
    #endregion

    #region MONOBEHAVIOUR

    // When enabled, increase town population
    public void OnEnable()
    {
        // Register this house and update town population
        TownManager.Instance.RegisterHouse(this);
    }

    // When disabled, decrease town population
    public void OnDisable()
    {
        // Unregister this house and decrease town population
        TownManager.Instance.UnregisterHouse(this);

        // There are still residents assigned to this house
        if (_residents.Count > 0)
        {
            // Ask TownManager to reassign residents centrally
            // (it will release those that cannot be reassigned and adjust population)
            List<Villager> residentsCopy = new(_residents);
            TownManager.Instance.ReassignResidents(this, residentsCopy);

            // Clear this house's residents list
            _residents.Clear();
        }
    }
    #endregion

    #region PUBLIC METHODS
    public bool HasCapacityForNewResident()
    {
        return _residents.Count < _householdSize;
    }

    public bool AssignAndPlaceNewResident(Villager villager)
    {
        if (AssignNewResident(villager))
            PlaceAtRandomEntrance(villager);

        return AssignNewResident(villager);
    }

    public bool AssignNewResident(Villager villager)
    {
        // Max residents reached or already assigned
        if (_residents.Count >= _householdSize || _residents.Contains(villager))
            return false;

        _residents.Add(villager);
        villager.AssignHome(this);
        return true;
    }

    public void RemoveResident(Villager villager)
    {
        if (_residents.Contains(villager))
            _residents.Remove(villager);
    }
    #endregion
}
