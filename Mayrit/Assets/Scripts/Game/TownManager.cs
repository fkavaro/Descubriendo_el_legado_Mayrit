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

    /// <summary>
    /// Registers a workplace in the town.
    /// </summary>
    public void RegisterWorkplace(Workplace workplace)
    {
        RegisterBuilding(_workplaces, workplace);
    }

    /// <summary>   
    /// Unregisters a workplace from the town.
    /// </summary>
    public void UnregisterWorkplace(Workplace workplace)
    {
        UnregisterBuilding(_workplaces, workplace);
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

    /// <summary>
    /// Finds and returns the sanctuary Building nearest to the provided home.
    /// </summary>
    /// <param name="home">The house used as the reference point for distance calculations.</param>
    /// <returns>The nearest sanctuary Building, or null if none available.</returns>
    public Sanctuary GetNearestSanctuary(House home)
    {
        // Validate inputs: no sanctuaries configured or invalid home -> nothing to do
        if (_sanctuaries == null || _sanctuaries.Count == 0 || home == null)
            return null;

        Sanctuary nearestSanctuary = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 homePosition = home.transform.position;

        foreach (var sanctuary in _sanctuaries)
        {
            // Skip null or deactivated entries
            if (sanctuary == null || !sanctuary.gameObject.activeSelf)
                continue;

            // Use squared magnitude to avoid the cost of sqrt when comparing distances
            float distanceSqr = (sanctuary.transform.position - homePosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestSanctuary = sanctuary;
            }
        }

        return nearestSanctuary;
    }

    public Market GetRandomMarket()
    {
        if (_markets == null || _markets.Count == 0) return null;
        int randomIndex = UnityEngine.Random.Range(0, _markets.Count);
        return _markets[randomIndex];
    }

    public Spot GetRandomMarketStallSpot()
    {
        // Take random market
        Market market = _markets[UnityEngine.Random.Range(0, _markets.Count)];
        // Take random stall from market
        return market.GetRandomStall().GetRandomAccessSpot();
    }
    #endregion

    #region PRIVATE METHODS
    void OnMilestoneChanged(ProgressManager.Milestone milestone)
    {
        OnPopulationChanged?.Invoke(_population);
    }

    void RegisterBuilding<T>(List<T> buildings, T building, int populationDelta = 0)
    where T : AAssignedBuilding
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
    where T : AAssignedBuilding
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
    #endregion
}
