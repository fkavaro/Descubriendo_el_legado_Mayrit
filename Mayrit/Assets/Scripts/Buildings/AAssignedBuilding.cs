using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AAssignedBuilding : ABuilding
{
    #region EDITOR PROPERTIES
    [Header("Assigned Properties")]
    [SerializeField] int _capacity = 1;
    [SerializeField] protected List<Villager> _assignedVillagers = new();

    public int Capacity => _capacity;
    public bool AtMaxCapacity => _assignedVillagers.Count >= _capacity;
    #endregion

    #region INTERNAL PROPERTIES
    protected NPCPoolManager _npcPoolManager;
    #endregion

    #region ABSTRACT METHODS
    public abstract void ReassignVillagers(List<Villager> assigned);
    #endregion

    #region LIFE CYCLE
    public override void OnDisable()
    {
        if (TownManager != null)
        {
            UnregisterBuilding();

            // Reassign villagers if there are any
            if (_assignedVillagers.Count > 0)
            {
                var residentsCopy = new List<Villager>(_assignedVillagers);

                try
                {
                    ReassignVillagers(residentsCopy);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"AAssignedBuilding.OnDisable: Reassign failed: {ex}");
                }

                _assignedVillagers.Clear();
            }
        }
        // TownManager is not available (likely during teardown).
        else
        {
            // Return assigned to pool if possible, then clear.
            _npcPoolManager = ServiceLocator.Instance.Get<NPCPoolManager>();

            if (_assignedVillagers.Count > 0)
            {
                if (_npcPoolManager != null)
                {
                    // Iterate over a snapshot to avoid collection-modified exceptions:
                    var snapshot = _assignedVillagers.ToArray();
                    foreach (var villager in snapshot)
                    {
                        try { _npcPoolManager.ReturnVillagerToPool(villager); } catch { }
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

    public virtual void IncreaseCapacity(int increase)
    {
        _capacity += increase;
    }
    #endregion
}
