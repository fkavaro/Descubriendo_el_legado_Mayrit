using System;
using System.Collections.Generic;
using UnityEngine;

public class House : Building
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
        // Unregister this house and decrease town population (use ExistingInstance to avoid creating TownManager during teardown)
        var tm = TownManager.ExistingInstance;
        if (tm != null) tm.UnregisterHouse(this);

        // There are still residents assigned to this house
        if (_residents.Count > 0)
        {
            // Ask TownManager to reassign residents centrally
            // (it will release those that cannot be reassigned and adjust population)
            List<Villager> residentsCopy = new(_residents);
            //TownManager.Instance.ReassignResidents(this, residentsCopy);
            if (tm != null) tm.ReassignResidents(this, residentsCopy);

            // Clear this house's residents list
            _residents.Clear();
        }
    }
    #endregion

    #region PUBLIC METHODS
    public bool HasCapacityForNewResident => _residents.Count < _householdSize;

    public bool AssignNewResident(Villager villager)
    {
        // Max residents reached or already assigned
        if (_residents.Count >= _householdSize || _residents.Contains(villager))
            return false;

        _residents.Add(villager);
        return true;
    }

    public void RemoveResident(Villager villager)
    {
        if (_residents.Contains(villager))
            _residents.Remove(villager);
    }
    #endregion
}
