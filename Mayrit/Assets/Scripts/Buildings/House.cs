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
        // Safely unregister this house and decrease town population.
        // Use ExistingInstance to avoid creating TownManager during teardown.
        var tm = TownManager.ExistingInstance;
        if (tm != null)
        {
            tm.UnregisterHouse(this);

            // There are residents assigned to this house: ask TownManager to reassign them
            if (_residents.Count > 0)
            {
                List<Villager> residentsCopy = new(_residents);

                try
                {
                    tm.ReassignResidents(this, residentsCopy);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"House.OnDisable: ReassignResidents failed: {ex}");
                }

                _residents.Clear();
            }
        }
        else
        {
            // TownManager is not available (likely during teardown). Return residents to pool if possible, then clear.
            var pool = NPCPoolManager.ExistingInstance;
            if (_residents.Count > 0)
            {
                if (pool != null)
                {
                    // Iterate over a snapshot to avoid collection-modified exceptions:
                    var snapshot = _residents.ToArray();
                    foreach (var v in snapshot)
                    {
                        try { pool.ReturnVillagerToPool(v); } catch { }
                    }
                }
                _residents.Clear();
            }
        }
    }
    #endregion

    #region PUBLIC METHODS
    public bool AtMaxCapacity => _residents.Count >= _householdSize;

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
