using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TownManager : Singleton<TownManager>
{
    #region EDITOR PROPERTIES
    [Header("Town Stats")]
    public int _population;
    public List<House> _houses = new();
    public List<Workplace> _workplaces = new();

    [Header("Places of Interest")]
    public List<Sanctuary> _sanctuaries;
    public List<Market> _markets;
    #endregion

    #region INTERNAL PROPERTIES    
    /// <summary>
    /// Event fired when population changes. Provides the new population value.
    /// </summary>
    public event Action<int> OnPopulationChanged;
    #endregion

    #region MONOBEHAVIOUR
    void Start()
    {
        // Subscribe to milestone changes to update population accordingly
        ProgressManager.Instance.OnMilestoneChanged += OnMilestoneChanged;
    }
    void OnDestroy()
    {
        // Unsubscribe from milestone changes
        var pm = ProgressManager.ExistingInstance;
        if (pm != null)
            pm.OnMilestoneChanged -= OnMilestoneChanged;
    }
    #endregion

    #region PUBLIC METHODS 
    public void UpdatePopulation(int householdSize)
    {
        _population += householdSize;
    }

    /// <summary>
    /// Registers a house in the town and updates population accordingly.
    /// </summary>
    public void RegisterHouse(House house)
    {
        RegisterBuilding(_houses, house, house._capacity);
    }

    /// <summary>
    /// Unregisters a house from the town and updates population accordingly.
    /// </summary>
    public void UnregisterHouse(House house)
    {
        UnregisterBuilding(_houses, house, -house._capacity);
    }

    public void RegisterWorkplace(Workplace workplace)
    {
        RegisterBuilding(_workplaces, workplace);
    }

    public void UnregisterWorkplace(Workplace workplace)
    {
        UnregisterBuilding(_workplaces, workplace);
    }

    public void RegisterMarket(Market market)
    {
        RegisterBuilding(_markets, market);
    }

    public void UnregisterMarket(Market market)
    {
        UnregisterBuilding(_markets, market);
    }

    public void RegisterSanctuary(Sanctuary sanctuary)
    {
        RegisterBuilding(_sanctuaries, sanctuary);
    }

    public void UnregisterSanctuary(Sanctuary sanctuary)
    {
        UnregisterBuilding(_sanctuaries, sanctuary);
    }

    /// <summary>
    /// Returns a random registered house with capacity for a new resident. 
    /// Optionally excluding given house.
    /// </summary>
    /// <returns>Never null. If no house with free capacity found, returns a random house with increased capacity.</returns>
    public House GetHouse(House excludedHouse = null)
    {
        House house = GetBuildingWithFreeCapacity(_houses, excludedHouse);

        // If no house with free capacity found
        if (house == null && _houses.Count > 0)
        {
            // Return random house with increased capacity
            house = _houses[UnityEngine.Random.Range(0, _houses.Count)];
            house.IncreaseCapacity(1);
        }

        // Never reeturn null assured
        return house;
    }

    /// <returns>Random registered workplace with capacity for a new employee. 
    /// Optionally excluding given workplace.
    /// </returns>
    public Workplace GetWorkplaceWithFreeCapacity(Workplace excludedWorkplace = null)
    {
        return GetBuildingWithFreeCapacity(_workplaces, excludedWorkplace);
    }

    /// <summary>
    /// Attempts to reassign residents from a destroyed house to other houses with free capacity.
    /// If a resident cannot be reassigned, it will be returned to the NPC pool and population decremented.
    /// </summary>
    public void ReassignResidents(House previousHouse, List<Villager> residents)
    {
        if (residents == null || residents.Count == 0) return;
        Reassign(previousHouse, residents, _houses, (villager, house) =>
        {
            villager.AssignHome(house);
            house.AddNewAssigned(villager);
        });
    }

    /// <summary>
    /// Attempts to reassign employees from a closed workplace to other workplaces with free capacity.
    /// If an employee cannot be reassigned, it will be returned to the NPC pool.
    /// </summary>
    public void ReassignEmployees(Workplace previousWorkplace, List<Villager> employees)
    {
        if (employees == null || employees.Count == 0) return;
        Reassign(previousWorkplace, employees, _workplaces, (villager, workplace) =>
        {
            villager.AssignWorkplace(workplace);
            workplace.AddNewAssigned(villager);
        });
    }

    public Sanctuary GetNearestSanctuary(ABuilding other)
    {
        return GetNearestBuilding(other, _sanctuaries);
    }

    public Market GetNearestMarket(ABuilding other)
    {
        return GetNearestBuilding(other, _markets);
    }
    #endregion

    #region PRIVATE METHODS
    void OnMilestoneChanged(ProgressManager.Milestone milestone)
    {
        OnPopulationChanged?.Invoke(_population);
    }

    void RegisterBuilding<T>(List<T> buildings, T building, int populationDelta = 0)
    where T : ABuilding
    {
        if (building == null) return;
        if (buildings == null) return;
        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            if (populationDelta != 0) UpdatePopulation(populationDelta);
        }
    }

    void UnregisterBuilding<T>(List<T> buildings, T building, int populationDelta = 0)
    where T : ABuilding
    {
        if (building == null) return;
        if (buildings == null) return;
        if (buildings.Contains(building))
        {
            buildings.Remove(building);
            if (populationDelta != 0) UpdatePopulation(populationDelta);
        }
    }

    T GetBuildingWithFreeCapacity<T>(List<T> buildings, T excludedBuilding)
    where T : AAssignedBuilding
    {
        if (buildings == null || buildings.Count == 0) return null;
        List<T> candidates = new();
        foreach (var building in buildings)
        {
            if (building == excludedBuilding) continue;
            if (!building.AtMaxCapacity) candidates.Add(building);
        }
        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    void Reassign<T>(T previousBuilding, List<Villager> assignedVillagers, List<T> buildings, Action<Villager, T> assignAction)
    where T : AAssignedBuilding
    {
        if (assignedVillagers == null || assignedVillagers.Count == 0) return;
        foreach (var villager in assignedVillagers)
        {
            if (villager == null) continue;

            try
            {
                var target = GetBuildingWithFreeCapacity(buildings, previousBuilding);
                if (target != null)
                {
                    assignAction(villager, target);
                }
                else
                {
                    // No available target: return to pool
                    try { NPCPoolManager.Instance.ReturnVillagerToPool(villager); } catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Reassign: exception while reassigning {villager.name}: {ex}");
                try { NPCPoolManager.Instance.ReturnVillagerToPool(villager); } catch { }
            }
        }
    }

    T GetNearestBuilding<T>(ABuilding buildingCloseBy, List<T> buildings)
    where T : ABuilding
    {
        // Validate inputs: no buildings configured or invalid home -> nothing to do
        if (buildings == null || buildings.Count == 0 || buildingCloseBy == null)
            return null;

        T nearestBuilding = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 buildingCloseByPos = buildingCloseBy.transform.position;

        foreach (var building in buildings)
        {
            // Skip null or deactivated entries
            if (building == null || !building.gameObject.activeSelf)
                continue;

            // Use squared magnitude to avoid the cost of sqrt when comparing distances
            float distanceSqr = (building.transform.position - buildingCloseByPos).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestBuilding = building;
            }
        }

        return nearestBuilding;
    }
    #endregion
}
