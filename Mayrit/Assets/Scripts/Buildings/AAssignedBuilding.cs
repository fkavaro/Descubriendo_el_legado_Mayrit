using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AAssignedBuilding : ABuilding
{
    #region EDITOR PROPERTIES
    [Header("Assigned Properties")]
    public int _capacity = 1;
    [SerializeField]
    protected List<Villager> _assignedVillagers = new();
    public bool AtMaxCapacity => _assignedVillagers.Count >= _capacity;
    #endregion

    #region ABSTRACT METHODS
    public abstract void Reassign(List<Villager> residents);
    #endregion

    #region MONOBEHAVIOUR
    public override void OnDisable()
    {
        var tm = TownManager.ExistingInstance;
        if (tm != null)
        {
            UnregisterBuilding();

            // Reassign villagers if there are any
            if (_assignedVillagers.Count > 0)
            {
                var residentsCopy = new List<Villager>(_assignedVillagers);

                try
                {
                    Reassign(residentsCopy);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"House.OnDisable: Reassign failed: {ex}");
                }

                _assignedVillagers.Clear();
            }
        }
        // TownManager is not available (likely during teardown).
        else
        {
            // Return assigned to pool if possible, then clear.
            var pool = NPCPoolManager.ExistingInstance;
            if (_assignedVillagers.Count > 0)
            {
                if (pool != null)
                {
                    // Iterate over a snapshot to avoid collection-modified exceptions:
                    var snapshot = _assignedVillagers.ToArray();
                    foreach (var v in snapshot)
                    {
                        try { pool.ReturnVillagerToPool(v); } catch { }
                    }
                }
                _assignedVillagers.Clear();
            }
        }
    }
    #endregion

    #region PUBLIC METHODS
    public bool AddNewAssigned(Villager villager)
    {
        // Max assigned reached or already assigned
        if (_assignedVillagers.Count >= _capacity || _assignedVillagers.Contains(villager))
            return false;

        _assignedVillagers.Add(villager);
        return true;
    }

    public void RemoveAssigned(Villager villager)
    {
        if (_assignedVillagers.Contains(villager))
            _assignedVillagers.Remove(villager);
    }
    #endregion
}
